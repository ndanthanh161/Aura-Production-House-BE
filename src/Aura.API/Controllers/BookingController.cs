using Aura.Application.Common;
using Aura.Application.DTOs.Booking;
using Aura.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Aura.API.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    [Authorize] // Yêu cầu đăng nhập cho tất cả endpoints
    public class BookingController : ControllerBase
    {
        private readonly IBookingService _bookingService;

        public BookingController(IBookingService bookingService)
        {
            _bookingService = bookingService;
        }

        // ──────────────────────────────────────────────────────────────────────
        // Helper: lấy userId của người đang gọi API từ JWT claim
        // ──────────────────────────────────────────────────────────────────────
        private Guid? GetCurrentUserId()
        {
            var raw = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return Guid.TryParse(raw, out var id) ? id : null;
        }

        private bool IsCustomer() =>
            User.FindFirstValue(ClaimTypes.Role) == "User";

        // ──────────────────────────────────────────────────────────────────────
        // GET api/v1/booking/schedules?from=&to=&staffId=
        // Customer: tu dong loc theo clientId cua minh.
        // Admin/Staff/Photographer: loc theo staffId (tuy chon).
        // ──────────────────────────────────────────────────────────────────────
        /// <summary>Xem lich chup — customer chi thay lich cua minh</summary>
        [HttpGet("schedules")]
        [Authorize(Roles = "User,Admin,Staff,Photographer")]
        public async Task<ActionResult<ApiResponse<IEnumerable<BookingScheduleResponseDTO>>>> GetSchedules(
            [FromQuery] DateTime? from,
            [FromQuery] DateTime? to,
            [FromQuery] Guid? staffId)
        {
            Guid? clientId = IsCustomer() ? GetCurrentUserId() : null;

            var schedules = await _bookingService.GetSchedulesAsync(clientId, staffId, from, to);
            return Ok(ApiResponse<IEnumerable<BookingScheduleResponseDTO>>.SuccessResponse(
                schedules, "Lấy danh sách lịch chụp thành công."));
        }

        // ──────────────────────────────────────────────────────────────────────
        // GET api/v1/booking/slots?date=2025-05-01&maxSlots=3
        // Tất cả user đã đăng nhập đều có thể kiểm tra slot trống.
        // ──────────────────────────────────────────────────────────────────────
        /// <summary>Kiểm tra slot trống theo ngày</summary>
        [HttpGet("slots")]
        public async Task<ActionResult<ApiResponse<SlotAvailabilityResponseDTO>>> CheckSlot(
            [FromQuery] DateTime date,
            [FromQuery] int maxSlots = 3)
        {
            var result = await _bookingService.CheckSlotAvailabilityAsync(date, maxSlots);
            return Ok(ApiResponse<SlotAvailabilityResponseDTO>.SuccessResponse(
                result,
                result.IsAvailable ? "Ngày này còn slot trống." : "Ngày này đã đầy lịch."));
        }

        // ──────────────────────────────────────────────────────────────────────
        // PATCH api/v1/booking/reschedule
        // Customer chỉ được đổi lịch dự án của mình (ownership tự động qua clientId).
        // Admin/Staff có thể đổi lịch bất kỳ dự án (clientId = null → bỏ qua ownership).
        // ──────────────────────────────────────────────────────────────────────
        /// <summary>Đổi lịch chụp</summary>
        [HttpPatch("reschedule")]
        [Authorize(Roles = "User,Admin,Staff,Photographer")]
        public async Task<ActionResult<ApiResponse<BookingScheduleResponseDTO>>> Reschedule(
            [FromBody] RescheduleRequestDTO request)
        {
            try
            {
                Guid? clientId = IsCustomer() ? GetCurrentUserId() : null;

                var result = await _bookingService.RescheduleAsync(request, clientId);
                if (result == null)
                    return NotFound(ApiResponse<BookingScheduleResponseDTO>.NotFoundResponse(
                        "Không tìm thấy lịch, dự án không thuộc về bạn, hoặc trạng thái không cho phép đổi lịch."));

                return Ok(ApiResponse<BookingScheduleResponseDTO>.SuccessResponse(
                    result, "Đổi lịch chụp thành công."));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ApiResponse<BookingScheduleResponseDTO>.ErrorResponse(ex.Message));
            }
        }

        // ──────────────────────────────────────────────────────────────────────
        // PATCH api/v1/booking/{id}/cancel
        // Customer chỉ được hủy dự án của mình.
        // Admin/Staff có thể hủy bất kỳ dự án.
        // ──────────────────────────────────────────────────────────────────────
        /// <summary>Hủy lịch chụp</summary>
        [HttpPatch("{id:guid}/cancel")]
        [Authorize(Roles = "User,Admin,Staff,Photographer")]
        public async Task<ActionResult<ApiResponse<string>>> Cancel(Guid id)
        {
            Guid? clientId = IsCustomer() ? GetCurrentUserId() : null;

            var result = await _bookingService.CancelBookingAsync(id, clientId);
            if (!result)
                return NotFound(ApiResponse<string>.NotFoundResponse(
                    "Không tìm thấy lịch, dự án không thuộc về bạn, hoặc dự án đã hoàn thành."));

            return Ok(ApiResponse<string>.SuccessResponse("Success", "Hủy lịch chụp thành công."));
        }

        // ──────────────────────────────────────────────────────────────────────
        // GET api/v1/booking/{id}/status
        // Customer chỉ xem được trạng thái dự án của mình.
        // ──────────────────────────────────────────────────────────────────────
        /// <summary>Trạng thái booking của một dự án</summary>
        [HttpGet("{id:guid}/status")]
        [Authorize(Roles = "User,Admin,Staff,Photographer")]
        public async Task<ActionResult<ApiResponse<BookingStatusResponseDTO>>> GetStatus(Guid id)
        {
            Guid? clientId = IsCustomer() ? GetCurrentUserId() : null;

            var result = await _bookingService.GetBookingStatusAsync(id, clientId);
            if (result == null)
                return NotFound(ApiResponse<BookingStatusResponseDTO>.NotFoundResponse(
                    "Không tìm thấy thông tin booking hoặc dự án không thuộc về bạn."));

            return Ok(ApiResponse<BookingStatusResponseDTO>.SuccessResponse(result));
        }
    }
}
