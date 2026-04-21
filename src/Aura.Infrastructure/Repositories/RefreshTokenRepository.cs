using Aura.Domain.Entity;
using Aura.Domain.Interfaces;
using Aura.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Aura.Infrastructure.Repositories;

public class RefreshTokenRepository : IRefreshTokenRepository
{
    private readonly AppDbContext _context;

    public RefreshTokenRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<RefreshToken?> GetByTokenAsync(string token)
    {
        return await _context.RefreshTokens
            .Include(rt => rt.User)
                .ThenInclude(u => u.Role)
            .FirstOrDefaultAsync(rt => rt.Token == token);
    }

    public async Task CreateAsync(RefreshToken refreshToken)
    {
        _context.RefreshTokens.Add(refreshToken);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(RefreshToken refreshToken)
    {
        _context.RefreshTokens.Remove(refreshToken);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAllByUserIdAsync(Guid userId)
    {
        var tokens = await _context.RefreshTokens
            .Where(rt => rt.UserId == userId)
            .ToListAsync();

        _context.RefreshTokens.RemoveRange(tokens);
        await _context.SaveChangesAsync();
    }
}
