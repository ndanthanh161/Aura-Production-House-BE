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
            return await _context.Projects
                .Include(p => p.Client)
                .Include(p => p.Staff)
                .Include(p => p.Package)
                .Include(p => p.Payments)
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<IEnumerable<Project>> GetAllAsync()
        {
            return await _context.Projects
                .Include(p => p.Client)
                .Include(p => p.Staff)
                .Include(p => p.Package)
                .Include(p => p.Payments)
                .ToListAsync();
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

        public async Task<bool> UpdateStaffAsync(Guid id, Guid staffId)
        {
            var project = await _context.Projects.FindAsync(id);
            if (project == null) return false;

            project.StaffId = staffId;
            project.UpdatedAt = DateTime.UtcNow;

            _context.Projects.Update(project);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<Project>> GetSchedulesAsync(Guid? clientId, Guid? staffId, DateTime? from, DateTime? to)
        {
            var query = _context.Projects
                .Include(p => p.Client)
                .Include(p => p.Staff)
                .Include(p => p.Package)
                .Include(p => p.Payments)
                .AsQueryable();

            if (clientId.HasValue)
            {
                query = query.Where(p => p.ClientId == clientId.Value);
            }

            if (staffId.HasValue)
                query = query.Where(p => p.StaffId == staffId.Value);

            if (from.HasValue)
                query = query.Where(p => p.Deadline >= from.Value.Date);

            if (to.HasValue)
                query = query.Where(p => p.Deadline <= to.Value.Date.AddDays(1).AddTicks(-1));

            return await query.OrderByDescending(p => p.CreatedAt).ToListAsync();
        }

        public async Task<IEnumerable<Project>> GetBookedOnDateAsync(DateTime date)
        {
            var dayStart = date.Date;
            var dayEnd = dayStart.AddDays(1).AddTicks(-1);

            return await _context.Projects
                .Where(p =>
                    (p.Status == ProjectStatus.Scheduled || p.Status == ProjectStatus.InProduction) &&
                    p.Deadline >= dayStart &&
                    p.Deadline <= dayEnd)
                .ToListAsync();
        }

        public async Task<Project?> RescheduleAsync(Guid projectId, DateTime newShootingDate)
        {
            var project = await _context.Projects.FindAsync(projectId);
            if (project == null) return null;

            if (project.Status == ProjectStatus.Completed ||
                project.Status == ProjectStatus.Cancelled) return null;

            project.Deadline = newShootingDate;
            // Không đổi Status - chỉ cập nhật thời gian
            project.UpdatedAt = DateTime.UtcNow;

            _context.Projects.Update(project);
            await _context.SaveChangesAsync();
            return project;
        }

        public async Task<bool> CancelAsync(Guid projectId)
        {
            var project = await _context.Projects.FindAsync(projectId);
            if (project == null) return false;

            if (project.Status == ProjectStatus.Completed) return false;

            project.Status = ProjectStatus.Cancelled;
            project.UpdatedAt = DateTime.UtcNow;

            _context.Projects.Update(project);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
