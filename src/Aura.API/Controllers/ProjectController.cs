using Aura.Application.Common; // Thêm namespace này
using Aura.Application.DTOs.Project;
using Aura.Application.Interfaces;
using Aura.Domain.Enum;
using Microsoft.AspNetCore.Mvc;

namespace Aura.API.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
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
        public async Task<ActionResult<ApiResponse<IEnumerable<ProjectResponseDTO>>>> GetAllProjects()
        {
            var projects = await _projectService.GetAllProjectsAsync();
            return Ok(ApiResponse<IEnumerable<ProjectResponseDTO>>.SuccessResponse(projects));
        }

        [HttpPut]
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
        public async Task<ActionResult<ApiResponse<string>>> UpdateStatus(Guid id, [FromQuery] ProjectStatus status)
        {
            var result = await _projectService.UpdateProjectStatusAsync(id, status);
            if (!result)
            {
                return NotFound(ApiResponse<string>.NotFoundResponse("Dự án không tồn tại."));
            }

            return Ok(ApiResponse<string>.SuccessResponse("Success", "Cập nhật trạng thái thành công."));
        }
    }
}
