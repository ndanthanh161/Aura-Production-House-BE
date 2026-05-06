using Aura.Application.Common;
using Aura.Application.DTOs.Portfolio;
using Aura.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Aura.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PortfolioController : ControllerBase
    {
        private readonly IPortfolioService _portfolioService;

        public PortfolioController(IPortfolioService portfolioService)
        {
            _portfolioService = portfolioService;
        }

        // ─── Public Endpoints ────────────────────────────────

        /// <summary>
        /// Get all published portfolio items (for public website)
        /// </summary>
        [HttpGet("published")]
        public async Task<IActionResult> GetPublished()
        {
            var items = await _portfolioService.GetPublishedAsync();
            return Ok(ApiResponse<IEnumerable<PortfolioItemResponseDTO>>.SuccessResponse(items, "Published portfolio items retrieved."));
        }

        /// <summary>
        /// Get a specific portfolio item by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var item = await _portfolioService.GetByIdAsync(id);
            if (item == null)
                return NotFound(ApiResponse<object>.ErrorResponse("Portfolio item not found.", 404));

            return Ok(ApiResponse<PortfolioItemResponseDTO>.SuccessResponse(item, "Portfolio item retrieved."));
        }

        // ─── Admin Endpoints ─────────────────────────────────

        /// <summary>
        /// Get all portfolio items (including unpublished, for admin)
        /// </summary>
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAll()
        {
            var items = await _portfolioService.GetAllAsync();
            return Ok(ApiResponse<IEnumerable<PortfolioItemResponseDTO>>.SuccessResponse(items, "All portfolio items retrieved."));
        }

        /// <summary>
        /// Create a new portfolio item
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([FromBody] CreatePortfolioRequestDTO request)
        {
            var item = await _portfolioService.CreateAsync(request);
            return StatusCode(201, ApiResponse<PortfolioItemResponseDTO>.CreatedResponse(item, "Portfolio item created."));
        }

        /// <summary>
        /// Update a portfolio item
        /// </summary>
        [HttpPut]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update([FromBody] UpdatePortfolioRequestDTO request)
        {
            var item = await _portfolioService.UpdateAsync(request);
            if (item == null)
                return NotFound(ApiResponse<object>.ErrorResponse("Portfolio item not found.", 404));

            return Ok(ApiResponse<PortfolioItemResponseDTO>.SuccessResponse(item, "Portfolio item updated."));
        }

        /// <summary>
        /// Toggle publish/unpublish a portfolio item
        /// </summary>
        [HttpPatch("{id}/toggle-publish")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> TogglePublish(Guid id)
        {
            var result = await _portfolioService.TogglePublishAsync(id);
            if (!result)
                return NotFound(ApiResponse<object>.ErrorResponse("Portfolio item not found.", 404));

            return Ok(ApiResponse<object>.SuccessResponse(null!, "Publish status toggled."));
        }

        /// <summary>
        /// Delete a portfolio item and all its media
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var result = await _portfolioService.DeleteAsync(id);
            if (!result)
                return NotFound(ApiResponse<object>.ErrorResponse("Portfolio item not found.", 404));

            return Ok(ApiResponse<object>.SuccessResponse(null!, "Portfolio item deleted."));
        }

        /// <summary>
        /// Upload media (image/video) to a portfolio item via Cloudinary
        /// </summary>
        [HttpPost("{id}/media")]
        [Authorize(Roles = "Admin")]
        [RequestSizeLimit(100 * 1024 * 1024)] // 100MB max for videos
        public async Task<IActionResult> UploadMedia(Guid id, IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest(ApiResponse<object>.ErrorResponse("File is required.", 400));

            var allowedTypes = new[] { "image/jpeg", "image/png", "image/webp", "video/mp4", "video/quicktime", "video/webm" };
            if (!allowedTypes.Contains(file.ContentType.ToLower()))
                return BadRequest(ApiResponse<object>.ErrorResponse("Only JPEG, PNG, WebP images and MP4, MOV, WebM videos are allowed.", 400));

            try
            {
                var media = await _portfolioService.UploadMediaAsync(id, file);
                return Ok(ApiResponse<PortfolioMediaResponseDTO>.SuccessResponse(media, "Media uploaded successfully."));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message, 400));
            }
        }

        /// <summary>
        /// Get Cloudinary signature for direct upload
        /// </summary>
        [HttpGet("upload-signature")]
        [Authorize(Roles = "Admin")]
        public IActionResult GetUploadSignature([FromQuery] string folder = "portfolio")
        {
            var signature = _portfolioService.GetUploadSignature(folder);
            return Ok(ApiResponse<object>.SuccessResponse(signature));
        }

        /// <summary>
        /// Save media info after direct upload to Cloudinary
        /// </summary>
        [HttpPost("{id}/media-direct")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AddMediaDirect(Guid id, [FromBody] DirectUploadRequest request)
        {
            try
            {
                var media = await _portfolioService.AddMediaDirectAsync(id, request.Url, request.PublicId, request.MediaType);
                return Ok(ApiResponse<PortfolioMediaResponseDTO>.SuccessResponse(media, "Media info saved."));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message, 400));
            }
        }

        public class DirectUploadRequest
        {
            public string Url { get; set; } = string.Empty;
            public string PublicId { get; set; } = string.Empty;
            public string MediaType { get; set; } = string.Empty;
        }

        /// <summary>
        /// Delete a specific media file from portfolio item
        /// </summary>
        [HttpDelete("media/{mediaId}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteMedia(Guid mediaId)
        {
            var result = await _portfolioService.DeleteMediaAsync(mediaId);
            if (!result)
                return NotFound(ApiResponse<object>.ErrorResponse("Media not found.", 404));

            return Ok(ApiResponse<object>.SuccessResponse(null!, "Media deleted."));
        }
    }
}
