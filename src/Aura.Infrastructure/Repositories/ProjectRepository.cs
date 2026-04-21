using Aura.Domain.Entity;
using Aura.Domain.Enum;
using Aura.Domain.Interfaces;
using Aura.Infrastructure.Data; // Thay db context namespace
using Microsoft.EntityFrameworkCore;

namespace Aura.Infrastructure.Repositories
{
    public class ProjectRepository : IProjectRepository
    {
        private readonly AppDbContext _context;

        public ProjectRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Project> AddAsync(Project project)
        {
            await _context.Projects.AddAsync(project);
            await _context.SaveChangesAsync();
            return project;
        }

        public async Task<Project?> GetByIdAsync(Guid id)
        {
            return await _context.Projects.FindAsync(id);
        }

        public async Task<IEnumerable<Project>> GetAllAsync()
        {
            return await _context.Projects.ToListAsync();
        }

        public async Task<Project> UpdateAsync(Project project)
        {
            _context.Projects.Update(project);
            await _context.SaveChangesAsync();
            return project;
        }

        public async Task<bool> UpdateStatusAsync(Guid id, ProjectStatus status)
        {
            var project = await _context.Projects.FindAsync(id);
            if (project == null) return false;

            project.Status = status;
            project.UpdatedAt = DateTime.UtcNow;

            _context.Projects.Update(project);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
