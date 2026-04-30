using Aura.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Aura.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
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
                return BadRequest("Message cannot be empty.");

            var response = await _chatService.ProcessMessageAsync(request.Message);
            return Ok(new { response });
        }

        [HttpPost("ingest")]
        public async Task<IActionResult> Ingest([FromBody] IngestRequest request)
        {
            await _chatService.IngestKnowledgeAsync(request.Content, request.Category);
            return Ok("Knowledge ingested successfully.");
        }

        [HttpGet("knowledge")]
        public async Task<IActionResult> GetKnowledge()
        {
            var knowledge = await _chatService.GetKnowledgeBaseAsync();
            return Ok(knowledge);
        }

        [HttpDelete("knowledge/{id}")]
        public async Task<IActionResult> DeleteKnowledge(Guid id)
        {
            await _chatService.DeleteKnowledgeAsync(id);
            return Ok("Knowledge deleted successfully.");
        }

        [HttpGet("logs")]
        public async Task<IActionResult> GetLogs()
        {
            var logs = await _chatService.GetChatLogsAsync();
            return Ok(logs);
        }

        [HttpPost("logs/{id}/toggle-pin")]
        public async Task<IActionResult> TogglePin(Guid id)
        {
            await _chatService.ToggleChatLogPinAsync(id);
            return Ok("Pin toggled successfully.");
        }
    }

    public class ChatRequest
    {
        public string Message { get; set; } = string.Empty;
    }

    public class IngestRequest
    {
        public string Content { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
    }
}
