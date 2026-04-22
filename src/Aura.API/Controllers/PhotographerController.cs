using Aura.Application.Common;
using Aura.Application.DTOs.User;
using Aura.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Aura.API.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class PhotographerController : ControllerBase
    {
        private readonly IPhotographerService _photographerService;

        public PhotographerController(IPhotographerService photographerService)
        {
            _photographerService = photographerService;
        }

        // GET api/v1/photographer
        /// <summary>Lấy danh sách tất cả photographer (Staff + Photographer role)</summary>
        [HttpGet]
        [Authorize(Roles = "Admin,Staff,Photographer")]
        public async Task<ActionResult<ApiResponse<IEnumerable<UserResponseDTO>>>> GetAll()
        {
            var photographers = await _photographerService.GetAllPhotographersAsync();
            return Ok(ApiResponse<IEnumerable<UserResponseDTO>>.SuccessResponse(
                photographers, "Lay danh sach photographer thanh cong."));
        }

        // GET api/v1/photographer/{id}
        /// <summary>Chi tiết một photographer</summary>
        [HttpGet("{id:guid}")]
        [Authorize(Roles = "Admin,Staff,Photographer")]
        public async Task<ActionResult<ApiResponse<UserResponseDTO>>> GetById(Guid id)
        {
            var photographer = await _photographerService.GetPhotographerByIdAsync(id);
            if (photographer == null)
                return NotFound(ApiResponse<UserResponseDTO>.NotFoundResponse(
                    "Khong tim thay photographer."));

            return Ok(ApiResponse<UserResponseDTO>.SuccessResponse(photographer));
        }

        // PUT api/v1/photographer
        /// <summary>Cập nhật thông tin photographer (Admin hoặc chính photographer đó)</summary>
        [HttpPut]
        [Authorize(Roles = "Admin,Photographer")]
        public async Task<ActionResult<ApiResponse<UserResponseDTO>>> Update(
            [FromBody] UpdateUserRequestDTO request)
        {
            var updated = await _photographerService.UpdatePhotographerAsync(request);
            if (updated == null)
                return NotFound(ApiResponse<UserResponseDTO>.NotFoundResponse(
                    "Khong tim thay photographer de cap nhat."));

            return Ok(ApiResponse<UserResponseDTO>.SuccessResponse(
                updated, "Cap nhat thong tin photographer thanh cong."));
        }

        // PATCH api/v1/photographer/{photographerId}/assign/{projectId}
        /// <summary>Phân công photographer cho dự án (Admin only)</summary>
        [HttpPatch("{photographerId:guid}/assign/{projectId:guid}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ApiResponse<string>>> AssignToProject(
            Guid photographerId, Guid projectId)
        {
            var result = await _photographerService.AssignToProjectAsync(photographerId, projectId);
            if (!result)
                return NotFound(ApiResponse<string>.NotFoundResponse(
                    "Khong tim thay photographer hoac du an."));

            return Ok(ApiResponse<string>.SuccessResponse(
                "Assigned", "Phan cong photographer thanh cong."));
        }

        // PATCH api/v1/photographer/{id}/deactivate
        /// <summary>Vô hiệu hóa photographer (Admin only)</summary>
        [HttpPatch("{id:guid}/deactivate")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ApiResponse<string>>> Deactivate(Guid id)
        {
            var result = await _photographerService.DeactivatePhotographerAsync(id);
            if (!result)
                return NotFound(ApiResponse<string>.NotFoundResponse(
                    "Khong tim thay photographer."));

            return Ok(ApiResponse<string>.SuccessResponse(
                "Deactivated", "Da vo hieu hoa photographer."));
        }
    }
}
