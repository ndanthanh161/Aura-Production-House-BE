using Aura.Application.DTOs.Contact;
using Aura.Application.Interfaces;
using Aura.Application.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Aura.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ContactController : ControllerBase
    {
        private readonly IContactMessageService _contactService;

        public ContactController(IContactMessageService contactService)
        {
            _contactService = contactService;
        }

        /// <summary>
        /// Send a contact message (Public)
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> SendMessage([FromBody] ContactMessageRequestDTO request)
        {
            var result = await _contactService.SendMessageAsync(request);
            return Ok(ApiResponse<ContactMessageResponseDTO>.SuccessResponse(result, "Message sent successfully."));
        }

        /// <summary>
        /// Get all messages (Admin)
        /// </summary>
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAll()
        {
            var messages = await _contactService.GetAllMessagesAsync();
            return Ok(ApiResponse<IEnumerable<ContactMessageResponseDTO>>.SuccessResponse(messages));
        }

        /// <summary>
        /// Mark a message as read (Admin)
        /// </summary>
        [HttpPatch("{id}/read")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> MarkAsRead(Guid id)
        {
            var result = await _contactService.MarkAsReadAsync(id);
            if (!result) return NotFound(ApiResponse<object>.ErrorResponse("Message not found.", 404));

            return Ok(ApiResponse<object>.SuccessResponse(null!, "Message marked as read."));
        }

        /// <summary>
        /// Delete a message (Admin)
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var result = await _contactService.DeleteMessageAsync(id);
            if (!result) return NotFound(ApiResponse<object>.ErrorResponse("Message not found.", 404));

            return Ok(ApiResponse<object>.SuccessResponse(null!, "Message deleted."));
        }
    }
}
