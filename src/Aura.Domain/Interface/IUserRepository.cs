namespace Aura.Domain.Interfaces;

public interface IUserRepository
{
    Task<Entity.User?> GetByEmailAsync(string email);
    Task<Entity.User?> GetByIdAsync(Guid id);
    Task<bool> ExistsByEmailAsync(string email);
    Task<Entity.User> CreateAsync(Entity.User user);
}
