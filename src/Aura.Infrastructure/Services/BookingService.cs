using Aura.Application.DTOs.Booking;
using Aura.Application.Interfaces;
using Aura.Domain.Entity;
using Aura.Domain.Enum;
using Aura.Domain.Interfaces;

namespace Aura.Infrastructure.Services
{
    public class BookingService : IBookingService
    {
        private readonly IBookingRepository _bookingRepository;

        // Số buổi chụp tối đa trong một ngày (có thể config sau)
        private const int DefaultMaxSlotsPerDay = 3;

        public BookingService(IBookingRepository bookingRepository)
        {
            _bookingRepository = bookingRepository;
        }

        // ─── 1. Quản lý lịch chụp ─────────────────────────────────────────────
        // Customer xem lịch của chính mình qua clientId.
        // Admin/Staff truyền staffId để lọc theo nhân viên phụ trách.
        public async Task<IEnumerable<BookingScheduleResponseDTO>> GetSchedulesAsync(
            Guid? clientId, Guid? staffId, DateTime? from, DateTime? to)
        {
            var projects = await _bookingRepository.GetSchedulesAsync(clientId, staffId, from, to);
            return projects.Select(MapToScheduleDTO);
        }

        // ─── 2. Kiểm tra slot trống ────────────────────────────────────────────
        public async Task<SlotAvailabilityResponseDTO> CheckSlotAvailabilityAsync(
            DateTime date, int maxSlotsPerDay = DefaultMaxSlotsPerDay)
        {
            var booked = (await _bookingRepository.GetBookedOnDateAsync(date)).ToList();

            return new SlotAvailabilityResponseDTO
            {
                Date = date.Date,
                BookedCount = booked.Count,
                IsAvailable = booked.Count < maxSlotsPerDay,
                BookedProjectIds = booked.Select(p => p.Id)
            };
        }

        // ─── 3. Đổi lịch ──────────────────────────────────────────────────────
        // clientId được truyền vào từ controller (lấy từ JWT) → ownership check tự động ở Repository.
        public async Task<BookingScheduleResponseDTO?> RescheduleAsync(
            RescheduleRequestDTO request, Guid? clientId = null)
        {
            if (request.NewShootingDate.ToUniversalTime() < DateTime.UtcNow)
                throw new ArgumentException("Ngày chụp mới không được trong quá khứ.");

            var updated = await _bookingRepository.RescheduleAsync(
                request.ProjectId, request.NewShootingDate.ToUniversalTime(), clientId);

            return updated == null ? null : MapToScheduleDTO(updated);
        }

        // ─── 4. Hủy lịch ──────────────────────────────────────────────────────
        public async Task<bool> CancelBookingAsync(Guid projectId, Guid? clientId = null)
        {
            return await _bookingRepository.CancelAsync(projectId, clientId);
        }

        // ─── 5. Trạng thái booking ─────────────────────────────────────────────
        public async Task<BookingStatusResponseDTO?> GetBookingStatusAsync(
            Guid projectId, Guid? clientId = null)
        {
            var project = await _bookingRepository.GetBookingStatusAsync(projectId, clientId);
            if (project == null) return null;

            var now = DateTime.UtcNow;

            // Cho phép hủy nếu còn ≥ 24h trước ngày chụp và chưa kết thúc
            var isCancellable = project.Status != ProjectStatus.Completed
                && project.Status != ProjectStatus.Cancelled
                && project.Deadline > now.AddHours(24);

            // Cho phép đổi lịch nếu còn ≥ 48h
            var isReschedulable = project.Status != ProjectStatus.Completed
                && project.Status != ProjectStatus.Cancelled
                && project.Deadline > now.AddHours(48);

            return new BookingStatusResponseDTO
            {
                ProjectId = project.Id,
                ProjectName = project.Name,
                Status = project.Status,
                StatusLabel = GetStatusLabel(project.Status),
                ShootingDate = project.Deadline,
                IsCancellable = isCancellable,
                IsReschedulable = isReschedulable,
                UpdatedAt = project.UpdatedAt
            };
        }

        // ─── Helpers ───────────────────────────────────────────────────────────

        private static BookingScheduleResponseDTO MapToScheduleDTO(Project project)
        {
            return new BookingScheduleResponseDTO
            {
                ProjectId = project.Id,
                ProjectName = project.Name,
                ClientId = project.ClientId,
                ClientName = project.Client?.FullName ?? string.Empty,
                StaffId = project.StaffId,
                StaffName = project.Staff?.FullName,
                PackageId = project.PackageId,
                PackageName = project.Package?.Name ?? string.Empty,
                ShootingDate = project.Deadline,
                Status = project.Status,
                Description = project.Description,
                CreatedAt = project.CreatedAt,
                UpdatedAt = project.UpdatedAt
            };
        }

        private static string GetStatusLabel(ProjectStatus status) => status switch
        {
            ProjectStatus.PreProduction => "Chờ thanh toán",
            ProjectStatus.InProduction  => "Đang thực hiện",
            ProjectStatus.Scheduled     => "Đã lên lịch",
            ProjectStatus.Completed     => "Hoàn thành",
            ProjectStatus.Cancelled     => "Đã hủy",
            _                           => status.ToString()
        };
    }
}
