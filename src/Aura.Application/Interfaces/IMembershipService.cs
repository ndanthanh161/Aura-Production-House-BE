using Aura.Application.Common;
using Aura.Application.DTOs.Membership;

namespace Aura.Application.Interfaces;

public interface IMembershipService
{
    Task<ApiResponse<MembershipOfferDTO>> GetWelcomeOfferAsync(Guid userId);
    Task<ApiResponse<MembershipOfferDTO>> ClaimWelcomeOfferAsync(Guid userId);
}
