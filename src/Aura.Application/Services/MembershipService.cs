using Aura.Application.Common;
using Aura.Application.DTOs.Membership;
using Aura.Application.Interfaces;
using Aura.Domain.Interfaces;

namespace Aura.Application.Services;

public class MembershipService : IMembershipService
{
    private readonly IUserRepository _userRepository;
    private readonly IPackageRepository _packageRepository;

    public MembershipService(
        IUserRepository userRepository,
        IPackageRepository packageRepository)
    {
        _userRepository = userRepository;
        _packageRepository = packageRepository;
    }

    public async Task<ApiResponse<MembershipOfferDTO>> GetWelcomeOfferAsync(Guid userId)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
        {
            return ApiResponse<MembershipOfferDTO>.NotFoundResponse("Không tìm thấy tài khoản.");
        }

        var membershipPackage = await GetMembershipPackageAsync();
        var isOfferEnabled = membershipPackage?.IsFreeMembershipOfferEnabled == true;
        return ApiResponse<MembershipOfferDTO>.SuccessResponse(ToOffer(user, isOfferEnabled));
    }

    public async Task<ApiResponse<MembershipOfferDTO>> ClaimWelcomeOfferAsync(Guid userId)
    {
        var membershipPackage = await GetMembershipPackageAsync();

        if (membershipPackage == null)
        {
            return ApiResponse<MembershipOfferDTO>.NotFoundResponse(
                "Gói Membership hiện không khả dụng.");
        }

        if (!membershipPackage.IsFreeMembershipOfferEnabled)
        {
            return ApiResponse<MembershipOfferDTO>.ForbiddenResponse(
                "Chương trình Membership miễn phí hiện đang tạm dừng.");
        }

        var claimedAt = DateTime.UtcNow;
        var user = await _userRepository.TryClaimFreeMembershipAsync(userId, claimedAt);
        if (user == null)
        {
            var existingUser = await _userRepository.GetByIdAsync(userId);
            if (existingUser == null)
            {
                return ApiResponse<MembershipOfferDTO>.NotFoundResponse("Không tìm thấy tài khoản.");
            }

            if (!existingUser.IsActive)
            {
                return ApiResponse<MembershipOfferDTO>.ForbiddenResponse("Tài khoản đã bị khóa.");
            }

            return ApiResponse<MembershipOfferDTO>.ErrorResponse(
                "Tài khoản đã nhận ưu đãi Membership miễn phí.",
                409,
                new List<string> { "WELCOME_MEMBERSHIP_ALREADY_CLAIMED" });
        }

        return ApiResponse<MembershipOfferDTO>.SuccessResponse(
            ToOffer(user, true),
            "Đã kích hoạt Membership miễn phí trong 1 tháng.");
    }

    private async Task<Aura.Domain.Entity.Package?> GetMembershipPackageAsync()
    {
        return (await _packageRepository.GetAllAsync())
            .FirstOrDefault(package =>
                package.Name.Trim().Equals("Membership", StringComparison.OrdinalIgnoreCase));
    }

    private static MembershipOfferDTO ToOffer(Aura.Domain.Entity.User user, bool isOfferEnabled)
    {
        var now = DateTime.UtcNow;
        return new MembershipOfferDTO
        {
            IsEligible = isOfferEnabled && user.IsActive && !user.HasClaimedFreeMembership,
            IsOfferEnabled = isOfferEnabled,
            HasClaimed = user.HasClaimedFreeMembership,
            IsActive = user.IsActive && user.IsVip &&
                       user.VipExpireAt.HasValue && user.VipExpireAt.Value > now,
            ClaimedAt = user.FreeMembershipClaimedAt,
            ExpiresAt = user.VipExpireAt
        };
    }
}
