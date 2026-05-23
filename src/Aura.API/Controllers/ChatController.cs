using Aura.Application.Common;
using Aura.Application.Interfaces;
using Aura.Application.DTOs.Chat;
using Microsoft.AspNetCore.Mvc;

namespace Aura.API.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class ChatController : ControllerBase
    {
        private readonly IChatService _chatService;

        public ChatController(IChatService chatService)
        {
            _chatService = chatService;
        }

        [HttpPost("message")]
        public async Task<IActionResult> SendMessage([FromBody] ChatRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Message))
                return BadRequest(ApiResponse<object>.ErrorResponse("Message cannot be empty."));

            var response = await _chatService.ProcessMessageAsync(request.Message, request.History);
            return Ok(ApiResponse<object>.SuccessResponse(new { response }, "AI response retrieved."));
        }

        [HttpPost("ingest")]
        public async Task<IActionResult> Ingest([FromBody] IngestRequest request)
        {
            await _chatService.IngestKnowledgeAsync(request.Content, request.Category);
            return Ok(ApiResponse<object>.SuccessResponse(null!, "Knowledge ingested successfully."));
        }

        [HttpGet("knowledge")]
        public async Task<IActionResult> GetKnowledge()
        {
            var knowledge = await _chatService.GetKnowledgeBaseAsync();
            return Ok(ApiResponse<object>.SuccessResponse(knowledge, "Knowledge base retrieved."));
        }

        [HttpDelete("knowledge/{id}")]
        public async Task<IActionResult> DeleteKnowledge(Guid id)
        {
            await _chatService.DeleteKnowledgeAsync(id);
            return Ok(ApiResponse<object>.SuccessResponse(null!, "Knowledge deleted successfully."));
        }

        [HttpGet("logs")]
        public async Task<IActionResult> GetLogs()
        {
            var logs = await _chatService.GetChatLogsAsync();
            return Ok(ApiResponse<object>.SuccessResponse(logs, "Chat logs retrieved."));
        }

        [HttpPost("logs/{id}/toggle-pin")]
        public async Task<IActionResult> TogglePin(Guid id)
        {
            await _chatService.ToggleChatLogPinAsync(id);
            return Ok(ApiResponse<object>.SuccessResponse(null!, "Pin toggled successfully."));
        }
    }

    public class ChatRequest
    {
        public string Message { get; set; } = string.Empty;
        public List<ChatMessageDTO>? History { get; set; }
    }

    public class IngestRequest
    {
        public string Content { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
    }
}
