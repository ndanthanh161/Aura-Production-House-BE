using Aura.Domain.Entity;
using Aura.Domain.Interfaces;
using Aura.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Aura.Infrastructure.Repositories;

public class ChatLogRepository : IChatLogRepository
{
    private readonly AppDbContext _context;
    public ChatLogRepository(AppDbContext context) => _context = context;

    public async Task<IEnumerable<ChatLog>> GetLatestLogsAsync(int count)
    {
        return await _context.ChatLogs
            .OrderByDescending(c => c.IsPinned)
            .ThenByDescending(c => c.CreatedAt)
            .Take(count)
            .ToListAsync();
    }

    public async Task<ChatLog?> GetByIdAsync(Guid id) => await _context.ChatLogs.FindAsync(id);
    
    public async Task AddAsync(ChatLog log) 
    { 
        await _context.ChatLogs.AddAsync(log); 
        await _context.SaveChangesAsync(); 
    }

    public async Task UpdateAsync(ChatLog log) 
    { 
        _context.ChatLogs.Update(log); 
        await _context.SaveChangesAsync(); 
    }
}
