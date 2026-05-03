using Aura.Application.Common;
using Aura.Application.DTOs.User;
using Aura.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Aura.API.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin,Staff")]
    public class CustomerController : ControllerBase
    {
        private readonly ICustomerService _customerService;

        public CustomerController(ICustomerService customerService)
        {
            _customerService = customerService;
        }

        // GET api/v1/customer
        /// <summary>Lấy danh sách tất cả khách hàng</summary>
        [HttpGet]
        public async Task<ActionResult<ApiResponse<IEnumerable<UserResponseDTO>>>> GetAll()
        {
            var customers = await _customerService.GetAllCustomersAsync();
            return Ok(ApiResponse<IEnumerable<UserResponseDTO>>.SuccessResponse(
                customers, "Lấy danh sách khách hàng thành công."));
        }

        // GET api/v1/customer/{id}
        /// <summary>Chi tiết một khách hàng</summary>
        [HttpGet("{id:guid}")]
        public async Task<ActionResult<ApiResponse<UserResponseDTO>>> GetById(Guid id)
        {
            var customer = await _customerService.GetCustomerByIdAsync(id);
            if (customer == null)
                return NotFound(ApiResponse<UserResponseDTO>.NotFoundResponse(
                    "Không tìm thấy khách hàng."));

            return Ok(ApiResponse<UserResponseDTO>.SuccessResponse(customer));
        }

        // PUT api/v1/customer
        /// <summary>Cập nhật thông tin khách hàng (Admin only)</summary>
        [HttpPut]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ApiResponse<UserResponseDTO>>> Update(
            [FromBody] UpdateUserRequestDTO request)
        {
            var updated = await _customerService.UpdateCustomerAsync(request);
            if (updated == null)
                return NotFound(ApiResponse<UserResponseDTO>.NotFoundResponse(
                    "Không tìm thấy khách hàng để cập nhật."));

            return Ok(ApiResponse<UserResponseDTO>.SuccessResponse(
                updated, "Cập nhật thông tin khách hàng thành công."));
        }

        // PATCH api/v1/customer/{id}/deactivate
        /// <summary>Vô hiệu hóa khách hàng (Admin only)</summary>
        [HttpPatch("{id:guid}/deactivate")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ApiResponse<string>>> Deactivate(Guid id)
        {
            var result = await _customerService.DeactivateCustomerAsync(id);
            if (!result)
                return NotFound(ApiResponse<string>.NotFoundResponse(
                    "Không tìm thấy khách hàng."));

            return Ok(ApiResponse<string>.SuccessResponse(
                "Deactivated", "Đã vô hiệu hóa khách hàng thành công."));
        }
    }
}
