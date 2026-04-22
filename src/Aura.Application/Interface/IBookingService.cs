using Aura.Application.DTOs.Booking;

namespace Aura.Application.Interfaces
{
    public interface IBookingService
    {
        /// <summary>
        /// Quản lý lịch chụp: customer truyền clientId của mình để chỉ xem lịch bản thân.
        /// Admin/Staff có thể bỏ qua clientId và lọc theo staffId.
        /// </summary>
        Task<IEnumerable<BookingScheduleResponseDTO>> GetSchedulesAsync(
            Guid? clientId, Guid? staffId, DateTime? from, DateTime? to);

        /// <summary>
        /// Kiểm tra slot trống theo ngày — public với mọi user đã đăng nhập.
        /// </summary>
        Task<SlotAvailabilityResponseDTO> CheckSlotAvailabilityAsync(DateTime date, int maxSlotsPerDay = 3);

        /// <summary>
        /// Đổi lịch chụp — customer truyền clientId để hệ thống validate quyền sở hữu.
        /// </summary>
        Task<BookingScheduleResponseDTO?> RescheduleAsync(RescheduleRequestDTO request, Guid? clientId = null);

        /// <summary>
        /// Hủy lịch chụp — customer truyền clientId để hệ thống validate quyền sở hữu.
        /// </summary>
        Task<bool> CancelBookingAsync(Guid projectId, Guid? clientId = null);

        /// <summary>
        /// Trạng thái booking — customer truyền clientId để chỉ xem dự án của mình.
        /// </summary>
        Task<BookingStatusResponseDTO?> GetBookingStatusAsync(Guid projectId, Guid? clientId = null);
    }
}
