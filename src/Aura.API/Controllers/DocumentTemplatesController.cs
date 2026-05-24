using Aura.Application.Common;
using Aura.Application.DTOs.DocumentTemplate;
using Aura.Application.Interfaces;
using Aura.Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Aura.API.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    [Authorize]
    public class DocumentTemplatesController : ControllerBase
    {
        private readonly IDocumentTemplateService _templateService;
        private readonly IUserRepository _userRepository;

        public DocumentTemplatesController(IDocumentTemplateService templateService, IUserRepository userRepository)
        {
            _templateService = templateService;
            _userRepository = userRepository;
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ApiResponse<DocumentTemplateResponseDTO>>> CreateTemplate([FromForm] CreateDocumentTemplateDTO request)
        {
            try
            {
                if (request.File == null || request.File.Length == 0)
                {
                    return BadRequest(ApiResponse<DocumentTemplateResponseDTO>.ErrorResponse("Vui lòng tải lên 1 tệp tài liệu."));
                }

                var extension = System.IO.Path.GetExtension(request.File.FileName).ToLower();
                if (extension != ".pdf" && extension != ".docx" && extension != ".doc")
                {
                    return BadRequest(ApiResponse<DocumentTemplateResponseDTO>.ErrorResponse("Định dạng file không hỗ trợ. Chỉ hỗ trợ .doc, .docx và .pdf"));
                }

                var result = await _templateService.CreateTemplateAsync(request);
                return StatusCode(201, ApiResponse<DocumentTemplateResponseDTO>.CreatedResponse(result, "Đăng tải tài liệu thành công."));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<DocumentTemplateResponseDTO>.ErrorResponse(ex.Message));
            }
        }

        [HttpGet]
        public async Task<ActionResult<ApiResponse<IEnumerable<DocumentTemplateResponseDTO>>>> GetAllTemplates()
        {
            var userIdStr = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var isAdmin = User.IsInRole("Admin");
            var isPhotographer = User.IsInRole("Photographer");
            
            bool hasVipAccess = isAdmin || isPhotographer;

            if (!hasVipAccess && Guid.TryParse(userIdStr, out var userId))
            {
                var user = await _userRepository.GetByIdAsync(userId);
                if (user != null && user.IsVip)
                {
                    if (user.VipExpireAt == null || user.VipExpireAt > DateTime.UtcNow)
                    {
                        hasVipAccess = true;
                    }
                }
            }

            // Lấy tất cả mẫu (Admin lấy cả bản nháp, User chỉ lấy bản đã công khai)
            var onlyPublished = !isAdmin;
            var result = await _templateService.GetAllTemplatesAsync(onlyPublished);

            // Bảo mật: Nếu không có đặc quyền VIP/Admin/Photographer, ẩn FileUrl đi
            if (!hasVipAccess)
            {
                result = result.Select(t => new DocumentTemplateResponseDTO
                {
                    Id = t.Id,
                    Title = t.Title,
                    Description = t.Description,
                    FileType = t.FileType,
                    IsPublished = t.IsPublished,
                    CreatedAt = t.CreatedAt,
                    UpdatedAt = t.UpdatedAt,
                    FileUrl = string.Empty // Ẩn FileUrl hoàn toàn để bảo mật!
                }).ToList();
            }

            return Ok(ApiResponse<IEnumerable<DocumentTemplateResponseDTO>>.SuccessResponse(result));
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<DocumentTemplateResponseDTO>>> GetTemplateById(Guid id)
        {
            var template = await _templateService.GetTemplateByIdAsync(id);
            if (template == null)
            {
                return NotFound(ApiResponse<DocumentTemplateResponseDTO>.NotFoundResponse("Không tìm thấy tài liệu này."));
            }

            var userIdStr = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var isAdmin = User.IsInRole("Admin");
            var isPhotographer = User.IsInRole("Photographer");

            bool hasVipAccess = isAdmin || isPhotographer;

            if (!hasVipAccess && Guid.TryParse(userIdStr, out var userId))
            {
                var user = await _userRepository.GetByIdAsync(userId);
                if (user != null && user.IsVip)
                {
                    if (user.VipExpireAt == null || user.VipExpireAt > DateTime.UtcNow)
                    {
                        hasVipAccess = true;
                    }
                }
            }

            // Nếu chưa được publish và không phải Admin
            if (!template.IsPublished && !isAdmin)
            {
                return NotFound(ApiResponse<DocumentTemplateResponseDTO>.NotFoundResponse("Tài liệu này không tồn tại hoặc chưa được xuất bản."));
            }

            // Bảo mật: Ẩn FileUrl nếu không có VIP
            if (!hasVipAccess)
            {
                template.FileUrl = string.Empty;
            }

            return Ok(ApiResponse<DocumentTemplateResponseDTO>.SuccessResponse(template));
        }

        [HttpPut]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ApiResponse<DocumentTemplateResponseDTO>>> UpdateTemplate([FromBody] UpdateDocumentTemplateDTO request)
        {
            var result = await _templateService.UpdateTemplateAsync(request);
            if (result == null)
            {
                return NotFound(ApiResponse<DocumentTemplateResponseDTO>.NotFoundResponse("Tài liệu không tồn tại."));
            }

            return Ok(ApiResponse<DocumentTemplateResponseDTO>.SuccessResponse(result, "Cập nhật tài liệu thành công."));
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ApiResponse<string>>> DeleteTemplate(Guid id)
        {
            var result = await _templateService.DeleteTemplateAsync(id);
            if (!result)
            {
                return NotFound(ApiResponse<string>.NotFoundResponse("Không tìm thấy tài liệu để xóa."));
            }

            return Ok(ApiResponse<string>.SuccessResponse("Deleted", "Đã xóa tài liệu và file đính kèm thành công."));
        }
    }
}
