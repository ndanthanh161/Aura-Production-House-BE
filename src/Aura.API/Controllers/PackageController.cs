using Aura.Application.Common; // Thêm namespace này
using Aura.Application.DTOs.Package;
using Aura.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Aura.API.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class PackageController : ControllerBase
    {
        private readonly IPackageService _packageService;

        public PackageController(IPackageService packageService)
        {
            _packageService = packageService;
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ApiResponse<PackageResponseDTO>>> CreatePackage([FromBody] CreatePackageRequestDTO request)
        {
            var newPackage = await _packageService.CreatePackageAsync(request);
            // Dùng CreatedResponse thay vì Ok object nặc danh
            return StatusCode(201, ApiResponse<PackageResponseDTO>.CreatedResponse(newPackage));
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<PackageResponseDTO>>> GetPackageById(Guid id)
        {
            var package = await _packageService.GetPackageByIdAsync(id);
            if (package == null)
            {
                return NotFound(ApiResponse<PackageResponseDTO>.NotFoundResponse("Không tìm thấy gói dịch vụ này."));
            }

            return Ok(ApiResponse<PackageResponseDTO>.SuccessResponse(package));
        }

        [HttpGet]
        public async Task<ActionResult<ApiResponse<IEnumerable<PackageResponseDTO>>>> GetAllPackages([FromQuery] bool all = false)
        {
            var packages = await _packageService.GetAllPackagesAsync(!all);
            return Ok(ApiResponse<IEnumerable<PackageResponseDTO>>.SuccessResponse(packages));
        }

        [HttpPut]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ApiResponse<PackageResponseDTO>>> UpdatePackage([FromBody] UpdatePackageRequestDTO request)
        {
            var updated = await _packageService.UpdatePackageAsync(request);
            if (updated == null)
            {
                return NotFound(ApiResponse<PackageResponseDTO>.NotFoundResponse("Không tìm thấy gói để cập nhật."));
            }

            return Ok(ApiResponse<PackageResponseDTO>.SuccessResponse(updated, "Cập nhật thành công."));
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ApiResponse<string>>> DeletePackage(Guid id)
        {
            var result = await _packageService.DeletePackageAsync(id);
            if (!result)
            {
                return NotFound(ApiResponse<string>.NotFoundResponse("Gói không tồn tại hoặc đã bị xóa."));
            }

            return Ok(ApiResponse<string>.SuccessResponse("Deleted", "Đã xóa Ẩn thành công."));
        }
    }
}
