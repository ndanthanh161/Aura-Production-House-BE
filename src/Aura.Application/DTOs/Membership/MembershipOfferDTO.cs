namespace Aura.Application.DTOs.Membership;

public class MembershipOfferDTO
{
    public bool IsEligible { get; set; }
    public bool HasClaimed { get; set; }
    public bool IsActive { get; set; }
    public DateTime? ClaimedAt { get; set; }
    public DateTime? ExpiresAt { get; set; }
}
