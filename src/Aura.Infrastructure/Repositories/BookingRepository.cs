using Aura.Domain.Entity;
using Aura.Domain.Enum;
using Aura.Domain.Interfaces;
using Aura.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Aura.Infrastructure.Repositories
{
    public class BookingRepository : IBookingRepository
    {
        private readonly AppDbContext _context;

        public BookingRepository(AppDbContext context)
        {
            _context = context;
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<Project>> GetSchedulesAsync(
            Guid? clientId, Guid? staffId, DateTime? from, DateTime? to)
        {
            var query = _context.Projects
                .Include(p => p.Client)
                .Include(p => p.Staff)
                .Include(p => p.Package)
                .AsQueryable();

            // Chỉ lấy project đã/đang có lịch chụp
            query = query.Where(p =>
                p.Status == ProjectStatus.Scheduled ||
                p.Status == ProjectStatus.InProduction);

            // Customer chỉ xem lịch của chính mình
            if (clientId.HasValue)
                query = query.Where(p => p.ClientId == clientId.Value);

            // Admin/Staff có thể lọc theo nhân viên phụ trách
            if (staffId.HasValue)
                query = query.Where(p => p.StaffId == staffId.Value);

            if (from.HasValue)
                query = query.Where(p => p.Deadline >= from.Value.Date);

            if (to.HasValue)
                query = query.Where(p => p.Deadline <= to.Value.Date.AddDays(1).AddTicks(-1));

            return await query.OrderBy(p => p.Deadline).ToListAsync();
        }

        /// <inheritdoc/>
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

        /// <inheritdoc/>
        public async Task<Project?> RescheduleAsync(
            Guid projectId, DateTime newShootingDate, Guid? clientId = null)
        {
            var project = await _context.Projects.FindAsync(projectId);
            if (project == null) return null;

            // Ownership check: customer chỉ được đổi lịch dự án của mình
            if (clientId.HasValue && project.ClientId != clientId.Value) return null;

            // Không cho đổi lịch khi đã hoàn thành hoặc đã hủy
            if (project.Status == ProjectStatus.Completed ||
                project.Status == ProjectStatus.Cancelled) return null;

            project.Deadline = newShootingDate;
            project.Status = ProjectStatus.Scheduled;
            project.UpdatedAt = DateTime.UtcNow;

            _context.Projects.Update(project);
            await _context.SaveChangesAsync();
            return project;
        }

        /// <inheritdoc/>
        public async Task<bool> CancelAsync(Guid projectId, Guid? clientId = null)
        {
            var project = await _context.Projects.FindAsync(projectId);
            if (project == null) return false;

            // Ownership check: customer chỉ được hủy dự án của mình
            if (clientId.HasValue && project.ClientId != clientId.Value) return false;

            // Không cho hủy project đã hoàn thành
            if (project.Status == ProjectStatus.Completed) return false;

            project.Status = ProjectStatus.Cancelled;
            project.UpdatedAt = DateTime.UtcNow;

            _context.Projects.Update(project);
            await _context.SaveChangesAsync();
            return true;
        }

        /// <inheritdoc/>
        public async Task<Project?> GetBookingStatusAsync(Guid projectId, Guid? clientId = null)
        {
            var query = _context.Projects
                .Include(p => p.Client)
                .Include(p => p.Staff)
                .Include(p => p.Package)
                .Where(p => p.Id == projectId);

            // Customer chỉ xem trạng thái dự án của mình
            if (clientId.HasValue)
                query = query.Where(p => p.ClientId == clientId.Value);

            return await query.FirstOrDefaultAsync();
        }
    }
}
