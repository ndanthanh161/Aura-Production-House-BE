using Aura.Application.DTOs.Project;
using Aura.Domain.Enum;

namespace Aura.Application.Interfaces
{
    public interface IProjectService
    {
        Task<ProjectResponseDTO> CreateProjectAsync(CreateProjectRequestDTO request);
        Task<ProjectResponseDTO?> GetProjectByIdAsync(Guid id);
        Task<IEnumerable<ProjectResponseDTO>> GetAllProjectsAsync();
        Task<ProjectResponseDTO?> UpdateProjectAsync(UpdateProjectRequestDTO request);
        Task<bool> UpdateProjectStatusAsync(Guid id, ProjectStatus status);
        Task<bool> UpdateProjectStaffAsync(Guid id, Guid staffId);

        // Migrated from Booking
        Task<IEnumerable<ProjectResponseDTO>> GetSchedulesAsync(Guid? clientId, Guid? staffId, DateTime? from, DateTime? to);
        Task<SlotAvailabilityResponseDTO> CheckSlotAvailabilityAsync(DateTime date, int maxSlotsPerDay = 3);
        Task<ProjectResponseDTO?> RescheduleAsync(RescheduleRequestDTO request);
        Task<bool> CancelProjectAsync(Guid projectId);
        Task<bool> HandlePaymentSuccessAsync(Guid projectId, decimal amount, string transactionId);
    }
}
