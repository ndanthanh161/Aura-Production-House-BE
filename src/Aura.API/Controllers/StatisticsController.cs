using Aura.Application.Common;
using Aura.Application.Interfaces;
using Aura.Application.DTOs.Statistics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Aura.API.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin,Photographer")]
    public class StatisticsController : ControllerBase
    {
        private readonly IStatisticsService _statisticsService;

        public StatisticsController(IStatisticsService statisticsService)
        {
            _statisticsService = statisticsService;
        }

        // GET api/v1/statistics/dashboard
        /// <summary>Thống kê tổng quan — số liệu chính cho dashboard</summary>
        [HttpGet("dashboard")]
        public async Task<ActionResult<ApiResponse<DashboardStatsDTO>>> GetDashboard()
        {
            var stats = await _statisticsService.GetDashboardStatsAsync();
            return Ok(ApiResponse<DashboardStatsDTO>.SuccessResponse(
                stats, "Lấy thống kê dashboard thành công."));
        }

        // GET api/v1/statistics/revenue?months=12
        /// <summary>Doanh thu theo tháng (dùng cho biểu đồ)</summary>
        [HttpGet("revenue")]
        public async Task<ActionResult<ApiResponse<IEnumerable<MonthlyRevenueDTO>>>> GetMonthlyRevenue(
            [FromQuery] int months = 12)
        {
            if (months < 1 || months > 60)
                return BadRequest(ApiResponse<IEnumerable<MonthlyRevenueDTO>>.ErrorResponse(
                    "Số tháng phải từ 1 đến 60."));

            var revenue = await _statisticsService.GetMonthlyRevenueAsync(months);
            return Ok(ApiResponse<IEnumerable<MonthlyRevenueDTO>>.SuccessResponse(
                revenue, "Lấy dữ liệu doanh thu thành công."));
        }

        // GET api/v1/statistics/photographer-performance
        /// <summary>Hiệu suất từng photographer (Admin only)</summary>
        [HttpGet("photographer-performance")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ApiResponse<IEnumerable<PhotographerPerformanceDTO>>>> GetPhotographerPerformance()
        {
            var performance = await _statisticsService.GetPhotographerPerformanceAsync();
            return Ok(ApiResponse<IEnumerable<PhotographerPerformanceDTO>>.SuccessResponse(
                performance, "Lấy thống kê hiệu suất photographer thành công."));
        }
    }
}
