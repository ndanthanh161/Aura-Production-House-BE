using Aura.Application.Interfaces;
using Aura.Application.Mappers;
using Aura.Domain.Entity;
using Aura.Domain.Interfaces;
using Pgvector;

namespace Aura.Application.Services;

public class ChatService : IChatService
{
    private readonly IAiService _aiService;
    private readonly IKnowledgeRepository _knowledgeRepo;
    private readonly IChatLogRepository _chatLogRepo;
    private readonly IPortfolioRepository _portfolioRepo;

    public ChatService(
        IAiService aiService, 
        IKnowledgeRepository knowledgeRepo, 
        IChatLogRepository chatLogRepo,
        IPortfolioRepository portfolioRepo)
    {
        _aiService = aiService;
        _knowledgeRepo = knowledgeRepo;
        _chatLogRepo = chatLogRepo;
        _portfolioRepo = portfolioRepo;
    }

    public async Task<string> ProcessMessageAsync(string message)
    {
        // 1. Get embedding for the user message
        var embedding = await _aiService.GetEmbeddingAsync(message);
        var vector = new Vector(embedding);

        // 2. Search for relevant context in Knowledge Base using Repository
        var relevantContext = await _knowledgeRepo.SearchRelevantContentAsync(vector, 8);

        // 3. Search for relevant Portfolio Projects using Repository (Logic sắp xếp đã đẩy xuống Repo)
        var portfolioItems = (await _portfolioRepo.GetTopHotPublishedAsync(5))
            .Select(p => $"Dự án mẫu: {p.Title} (Hạng mục: {p.Category}, Khách hàng: {p.ClientName ?? "Aura Client"})")
            .ToList();

        var contextString = string.Join("\n---\n", relevantContext);
        if (portfolioItems.Any())
        {
            contextString += "\n\nCÁC DỰ ÁN THỰC TẾ ĐÃ THỰC HIỆN (HÃY GỢI Ý KHI CẦN):\n" + string.Join("\n", portfolioItems);
        }

        // 4. Get response from AI
        var botResponse = await _aiService.GetChatResponseAsync(message, contextString);

        // 4. Save log to DB using Repository
        var log = ChatMapper.ToLogEntity(message, botResponse);
        await _chatLogRepo.AddAsync(log);

        return botResponse;
    }

    public async Task IngestKnowledgeAsync(string content, string category)
    {
        var embedding = await _aiService.GetEmbeddingAsync(content);
        var knowledge = ChatMapper.ToKnowledgeEntity(content, category, embedding);

        await _knowledgeRepo.AddAsync(knowledge);
    }

    public async Task<IEnumerable<AuraKnowledge>> GetKnowledgeBaseAsync()
    {
        return await _knowledgeRepo.GetAllAsync();

    }

    public async Task DeleteKnowledgeAsync(Guid id)
    {
        var knowledge = await _knowledgeRepo.GetByIdAsync(id);
        if (knowledge != null)
        {
            await _knowledgeRepo.DeleteAsync(knowledge);
        }
    }


    public async Task<IEnumerable<ChatLog>> GetChatLogsAsync()
    {
        return await _chatLogRepo.GetLatestLogsAsync(100);
    }


    public async Task ToggleChatLogPinAsync(Guid id)
    {
        var log = await _chatLogRepo.GetByIdAsync(id);
        if (log != null)
        {
            log.IsPinned = !log.IsPinned;
            await _chatLogRepo.UpdateAsync(log);
        }
    }
}
