using Aura.Domain.Entity;

namespace Aura.Domain.Interfaces;

public interface IChatLogRepository
{
    Task<IEnumerable<ChatLog>> GetLatestLogsAsync(int count);
    Task<ChatLog?> GetByIdAsync(Guid id);
    Task AddAsync(ChatLog log);
    Task UpdateAsync(ChatLog log);
}
