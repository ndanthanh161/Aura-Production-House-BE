using System.Linq;
using Aura.Application.Interfaces;
using Aura.Domain.Entity;
using Aura.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Pgvector;
using Pgvector.EntityFrameworkCore;

namespace Aura.Infrastructure.Services
{
    public class ChatService : IChatService
    {
        private readonly AppDbContext _context;
        private readonly IAiService _aiService;

        public ChatService(AppDbContext context, IAiService aiService)
        {
            _context = context;
            _aiService = aiService;
        }

        public async Task<string> ProcessMessageAsync(string message)
        {
            // 1. Get embedding for the user message
            var embedding = await _aiService.GetEmbeddingAsync(message);
            var vector = new Vector(embedding);

            // 2. Search for relevant context in DB
            var relevantContext = await _context.AuraKnowledge
                .OrderBy(k => k.Embedding.L2Distance(vector))
                .Take(3)
                .Select(k => k.Content)
                .ToListAsync();

            var contextString = string.Join("\n---\n", relevantContext);

            // 3. Get response from AI
            var botResponse = await _aiService.GetChatResponseAsync(message, contextString);

            // 4. Save log to DB
            var log = new ChatLog
            {
                Id = Guid.NewGuid(),
                UserMessage = message,
                BotResponse = botResponse,
                CreatedAt = DateTime.UtcNow
            };
            _context.ChatLogs.Add(log);
            await _context.SaveChangesAsync();

            return botResponse;
        }

        public async Task IngestKnowledgeAsync(string content, string category)
        {
            var embedding = await _aiService.GetEmbeddingAsync(content);
            
            var knowledge = new AuraKnowledge
            {
                Id = Guid.NewGuid(),
                Content = content,
                Category = category,
                Embedding = new Vector(embedding),
                CreatedAt = DateTime.UtcNow
            };

            _context.AuraKnowledge.Add(knowledge);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<AuraKnowledge>> GetKnowledgeBaseAsync()
        {
            return await _context.AuraKnowledge
                .OrderByDescending(k => k.CreatedAt)
                .ToListAsync();
        }

        public async Task DeleteKnowledgeAsync(Guid id)
        {
            var knowledge = await _context.AuraKnowledge.FindAsync(id);
            if (knowledge != null)
            {
                _context.AuraKnowledge.Remove(knowledge);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<ChatLog>> GetChatLogsAsync()
        {
            return await _context.ChatLogs
                .OrderByDescending(c => c.IsPinned) // Pinned items first
                .ThenByDescending(c => c.CreatedAt)
                .Take(100)
                .ToListAsync();
        }

        public async Task ToggleChatLogPinAsync(Guid id)
        {
            var log = await _context.ChatLogs.FindAsync(id);
            if (log != null)
            {
                log.IsPinned = !log.IsPinned;
                await _context.SaveChangesAsync();
            }
        }
    }
}
