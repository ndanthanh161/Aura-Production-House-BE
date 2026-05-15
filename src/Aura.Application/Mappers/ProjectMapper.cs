using Aura.Application.DTOs.Project;
using Aura.Domain.Entity;
using Aura.Domain.Enum;

namespace Aura.Application.Mappers;

public static class ProjectMapper
{
    public static Project ToEntity(CreateProjectRequestDTO request, Package package)
    {
        return new Project
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            ClientId = request.ClientId,
            PackageId = request.PackageId,
            Revenue = package.Price,
            Benefits = new List<string>(package.Benefits),
            Status = ProjectStatus.Scheduled,
            Deadline = DateTime.UtcNow.AddDays(7),
            Description = request.Description,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    public static ProjectResponseDTO ToDTO(Project project)
    {
        return new ProjectResponseDTO
        {
            Id = project.Id,
            Name = project.Name,
            ClientId = project.ClientId,
            ClientName = project.Client?.FullName ?? string.Empty,
            PackageId = project.PackageId,
            PackageName = project.Package?.Name ?? string.Empty,
            StaffId = project.StaffId == Guid.Empty ? null : project.StaffId,
            StaffName = project.Staff?.FullName,
            Status = project.Status,
            Revenue = project.Revenue,
            Benefits = project.Benefits,
            Deadline = project.Deadline,
            Description = project.Description,
            ResultLink = project.ResultLink,
            CreatedAt = project.CreatedAt,
            UpdatedAt = project.UpdatedAt
        };
    }

    public static void UpdateProject(Project project, UpdateProjectRequestDTO request)
    {
        project.Name = request.Name;
        project.StaffId = request.StaffId;
        project.Status = request.Status;
        project.Revenue = request.Revenue;
        project.Deadline = request.Deadline;
        project.Description = request.Description;
        project.ResultLink = request.ResultLink;
        project.UpdatedAt = DateTime.UtcNow;
    }
}
