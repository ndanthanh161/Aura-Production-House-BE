namespace Aura.Domain.Interfaces;

public interface IRefreshTokenRepository
{
    Task<Entity.RefreshToken?> GetByTokenAsync(string token);
    Task CreateAsync(Entity.RefreshToken refreshToken);
    Task DeleteAsync(Entity.RefreshToken refreshToken);
    Task DeleteAllByUserIdAsync(Guid userId);
}
