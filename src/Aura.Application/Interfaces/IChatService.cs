namespace Aura.Application.Interfaces
{
    public interface IChatService
    {
        Task<string> ProcessMessageAsync(string message);
        Task IngestKnowledgeAsync(string content, string category);
        Task<IEnumerable<Aura.Domain.Entity.AuraKnowledge>> GetKnowledgeBaseAsync();
        Task DeleteKnowledgeAsync(Guid id);
        Task<IEnumerable<Aura.Domain.Entity.ChatLog>> GetChatLogsAsync();
        Task ToggleChatLogPinAsync(Guid id);
    }
}
