using Aura.Application.Common;
using Aura.Application.DTOs.Payment;
using Aura.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace Aura.API.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class SePayController : ControllerBase
    {
        private readonly IProjectService _projectService;
        private readonly IConfiguration _configuration;

        public SePayController(IProjectService projectService, IConfiguration configuration)
        {
            _projectService = projectService;
            _configuration = configuration;
        }

        // GET api/v1/sepay/info
        [HttpGet("info")]
        public IActionResult GetInfo()
        {
            var data = new {
                bankId = _configuration["SePay:BankId"],
                accountNumber = _configuration["SePay:AccountNumber"],
                accountName = _configuration["SePay:AccountName"]
            };
            return Ok(ApiResponse<object>.SuccessResponse(data, "SePay info retrieved."));
        }

        // POST api/v1/sepay/webhook
        [HttpPost("webhook")]
        public async Task<IActionResult> Webhook([FromBody] SePayWebhookDTO request)
        {
            // 1. Verify API Key (Security)
            var authHeader = Request.Headers["Authorization"].ToString();
            var expectedKey = _configuration["SePay:ApiKey"];
            
            // SePay sends "Bearer <API_KEY>"
            if (string.IsNullOrEmpty(expectedKey) || !authHeader.Contains(expectedKey))
            {
                return Unauthorized(ApiResponse<object>.UnauthorizedResponse("Invalid SePay API Key"));
            }

            // 2. Extract Project ID from content
            var content = request.Content;
            Console.WriteLine($"[SePay Webhook] Received transaction. Content: {content}, Amount: {request.TransferAmount}");

            var projectId = ExtractProjectId(content);

            if (projectId == Guid.Empty)
            {
                Console.WriteLine($"[SePay Webhook] ERROR: Could not extract Project ID from content: {content}");
                return BadRequest(ApiResponse<object>.ErrorResponse("Could not find Project ID in transaction content"));
            }

            // 3. Handle Payment
            var result = await _projectService.HandlePaymentSuccessAsync(
                projectId, 
                request.TransferAmount, 
                request.Id.ToString());

            if (!result)
            {
                Console.WriteLine($"[SePay Webhook] ERROR: HandlePaymentSuccessAsync failed for Project ID: {projectId}");
                return NotFound(ApiResponse<object>.NotFoundResponse(ErrorMessages.ProjectNotFound));
            }

            Console.WriteLine($"[SePay Webhook] SUCCESS: Project {projectId} updated to InProduction");
            return Ok(ApiResponse<object>.SuccessResponse(new { success = true }, "Payment processed successfully."));
        }

        private Guid ExtractProjectId(string content)
        {
            if (string.IsNullOrWhiteSpace(content)) return Guid.Empty;

            // Simple search for Guid in string
            var parts = content.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            foreach (var part in parts)
            {
                if (Guid.TryParse(part, out var guid))
                {
                    return guid;
                }
            }

            // Try to find if the Guid is attached to "AURA" (e.g. AURA58a2bc...)
            if (content.ToUpper().StartsWith("AURA"))
            {
                var potentialId = content.Substring(4).Trim();
                if (Guid.TryParse(potentialId, out var guid))
                {
                    return guid;
                }
            }

            return Guid.Empty;
        }
    }
}
