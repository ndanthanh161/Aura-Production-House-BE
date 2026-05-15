using Aura.Domain.Entity;
using Pgvector;

namespace Aura.Application.Mappers;

public static class ChatMapper
{
    public static ChatLog ToLogEntity(string message, string botResponse)
    {
        return new ChatLog
        {
            Id = Guid.NewGuid(),
            UserMessage = message,
            BotResponse = botResponse,
            CreatedAt = DateTime.UtcNow
        };
    }

    public static AuraKnowledge ToKnowledgeEntity(string content, string category, float[] embedding)
    {
        return new AuraKnowledge
        {
            Id = Guid.NewGuid(),
            Content = content,
            Category = category,
            Embedding = new Vector(embedding),
            CreatedAt = DateTime.UtcNow
        };
    }
}
