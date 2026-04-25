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
        Task<bool> UpdateStaffAsync(Guid id, Guid staffId);
        
        // Booking logic migration
        Task<IEnumerable<Project>> GetSchedulesAsync(Guid? clientId, Guid? staffId, DateTime? from, DateTime? to);
        Task<IEnumerable<Project>> GetBookedOnDateAsync(DateTime date);
        Task<Project?> RescheduleAsync(Guid projectId, DateTime newShootingDate);
        Task<bool> CancelAsync(Guid projectId);
    }
}
