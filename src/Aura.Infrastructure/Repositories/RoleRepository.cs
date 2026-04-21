using Aura.Domain.Entity;
using Aura.Domain.Interfaces;
using Aura.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Aura.Infrastructure.Repositories;

public class RoleRepository : IRoleRepository
{
    private readonly AppDbContext _context;

    public RoleRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Role?> GetByNameAsync(string name)
    {
        return await _context.Roles.FirstOrDefaultAsync(r => r.Name == name);
    }
}
