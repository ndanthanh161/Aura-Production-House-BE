namespace Aura.Domain.Interfaces;

public interface IRoleRepository
{
    Task<Entity.Role?> GetByNameAsync(string name);
}
