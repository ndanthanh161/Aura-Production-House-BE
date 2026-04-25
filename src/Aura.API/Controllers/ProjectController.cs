using Aura.Application.Common; // Thêm namespace này
using Aura.Application.DTOs.Project;
using Aura.Application.Interfaces;
using Aura.Domain.Enum;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Aura.API.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    [Authorize] // Yêu cầu đăng nhập, cụ thể từng endpoint bên dưới
    public class ProjectController : ControllerBase
    {
        private readonly IProjectService _projectService;

        public ProjectController(IProjectService projectService)
        {
            _projectService = projectService;
        }

        [HttpPost]
        public async Task<ActionResult<ApiResponse<ProjectResponseDTO>>> CreateProject([FromBody] CreateProjectRequestDTO request)
        {
            try
            {
                var newProject = await _projectService.CreateProjectAsync(request);
                return StatusCode(201, ApiResponse<ProjectResponseDTO>.CreatedResponse(newProject, "Tạo dự án thành công. Đang chờ thanh toán!"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<ProjectResponseDTO>.ErrorResponse(ex.Message));
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<ProjectResponseDTO>>> GetProjectById(Guid id)
        {
            var project = await _projectService.GetProjectByIdAsync(id);
            if (project == null)
            {
                return NotFound(ApiResponse<ProjectResponseDTO>.NotFoundResponse("Không tìm thấy dự án."));
            }

            return Ok(ApiResponse<ProjectResponseDTO>.SuccessResponse(project));
        }

        [HttpGet]
        [Authorize(Roles = "Admin,Photographer")]
        public async Task<ActionResult<ApiResponse<IEnumerable<ProjectResponseDTO>>>> GetAllProjects()
        {
            var projects = await _projectService.GetAllProjectsAsync();
            return Ok(ApiResponse<IEnumerable<ProjectResponseDTO>>.SuccessResponse(projects));
        }

        [HttpPut]
        [Authorize(Roles = "Admin,Photographer")]
        public async Task<ActionResult<ApiResponse<ProjectResponseDTO>>> UpdateProject([FromBody] UpdateProjectRequestDTO request)
        {
            var updated = await _projectService.UpdateProjectAsync(request);
            if (updated == null)
            {
                return NotFound(ApiResponse<ProjectResponseDTO>.NotFoundResponse("Không tìm thấy dự án để cập nhật."));
            }

            return Ok(ApiResponse<ProjectResponseDTO>.SuccessResponse(updated, "Cập nhật dự án thành công."));
        }

        [HttpPatch("{id}/status")]
        [Authorize(Roles = "Admin,Photographer")]
        public async Task<ActionResult<ApiResponse<string>>> UpdateStatus(Guid id, [FromQuery] ProjectStatus status)
        {
            var result = await _projectService.UpdateProjectStatusAsync(id, status);
            if (!result)
            {
                return NotFound(ApiResponse<string>.NotFoundResponse("Dự án không tồn tại."));
            }

            return Ok(ApiResponse<string>.SuccessResponse("Success", "Cập nhật trạng thái thành công."));
        }

        [HttpPatch("{id}/assign-photographer")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ApiResponse<string>>> AssignPhotographer(Guid id, [FromQuery] Guid photographerId)
        {
            var result = await _projectService.UpdateProjectStaffAsync(id, photographerId);
            if (!result)
            {
                return NotFound(ApiResponse<string>.NotFoundResponse("Dự án không tồn tại."));
            }

            return Ok(ApiResponse<string>.SuccessResponse("Success", "Phân công thợ chụp ảnh thành công."));
        }

        [HttpGet("schedules")]
        public async Task<ActionResult<ApiResponse<IEnumerable<ProjectResponseDTO>>>> GetSchedules(
            [FromQuery] DateTime? from = null,
            [FromQuery] DateTime? to = null,
            [FromQuery] Guid? staffId = null)
        {
            var role = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;
            Guid? clientId = null;

            if (role == "User")
            {
                var userIdStr = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (Guid.TryParse(userIdStr, out var userId)) clientId = userId;
            }
            else if (role == "Photographer")
            {
                var staffIdStr = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (Guid.TryParse(staffIdStr, out var sId)) staffId = sId;
            }

            var result = await _projectService.GetSchedulesAsync(clientId, staffId, from, to);
            return Ok(ApiResponse<IEnumerable<ProjectResponseDTO>>.SuccessResponse(result));
        }

        [HttpGet("slots")]
        public async Task<ActionResult<ApiResponse<SlotAvailabilityResponseDTO>>> CheckSlot(
            [FromQuery] DateTime date,
            [FromQuery] int maxSlots = 3)
        {
            var result = await _projectService.CheckSlotAvailabilityAsync(date, maxSlots);
            return Ok(ApiResponse<SlotAvailabilityResponseDTO>.SuccessResponse(result));
        }

        [HttpPatch("reschedule")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ApiResponse<ProjectResponseDTO>>> Reschedule([FromBody] RescheduleRequestDTO request)
        {
            try
            {
                var updated = await _projectService.RescheduleAsync(request);
                if (updated == null) return NotFound(ApiResponse<ProjectResponseDTO>.NotFoundResponse("Không tìm thấy dự án để đổi lịch."));

                return Ok(ApiResponse<ProjectResponseDTO>.SuccessResponse(updated, "Đổi lịch chụp thành công."));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ApiResponse<ProjectResponseDTO>.ErrorResponse(ex.Message));
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ApiResponse<string>>> Cancel(Guid id)
        {
            var result = await _projectService.CancelProjectAsync(id);
            if (!result) return BadRequest(ApiResponse<string>.ErrorResponse("Không thể hủy dự án này."));

            return Ok(ApiResponse<string>.SuccessResponse("Success", "Hủy dự án thành công."));
        }
    }
}
