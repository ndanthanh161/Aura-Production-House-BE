using System.Security.Claims;
using Aura.Application.Common;
using Aura.Application.DTOs.Membership;
using Aura.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Aura.API.Controllers;

[Route("api/v1/[controller]")]
[ApiController]
[Authorize(Roles = "User")]
public class MembershipController : ControllerBase
{
    private readonly IMembershipService _membershipService;

    public MembershipController(IMembershipService membershipService)
    {
        _membershipService = membershipService;
    }

    [HttpGet("welcome-offer")]
    public async Task<ActionResult<ApiResponse<MembershipOfferDTO>>> GetWelcomeOffer()
    {
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized(ApiResponse<MembershipOfferDTO>.UnauthorizedResponse());
        }

        var result = await _membershipService.GetWelcomeOfferAsync(userId);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPost("claim-welcome")]
    public async Task<ActionResult<ApiResponse<MembershipOfferDTO>>> ClaimWelcomeOffer()
    {
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized(ApiResponse<MembershipOfferDTO>.UnauthorizedResponse());
        }

        var result = await _membershipService.ClaimWelcomeOfferAsync(userId);
        return StatusCode(result.StatusCode, result);
    }

    private bool TryGetUserId(out Guid userId)
    {
        return Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out userId);
    }
}
