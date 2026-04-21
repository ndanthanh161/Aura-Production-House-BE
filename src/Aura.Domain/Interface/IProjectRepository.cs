using Aura.Domain.Entity;
using Aura.Domain.Enum;

namespace Aura.Domain.Interfaces
{
    public interface IProjectRepository
    {
        Task<Project> AddAsync(Project project);
        Task<Project?> GetByIdAsync(Guid id);
        Task<IEnumerable<Project>> GetAllAsync();
        Task<Project> UpdateAsync(Project project);
        Task<bool> UpdateStatusAsync(Guid id, ProjectStatus status);
    }
}
