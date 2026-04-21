using Aura.Application.Common;
using Aura.Application.DTOs.Auth;

namespace Aura.Application.Interfaces;

public interface IAuthService
{
    Task<ApiResponse<AuthResponse>> RegisterAsync(RegisterRequest request);
    Task<ApiResponse<AuthResponse>> LoginAsync(LoginRequest request);
    Task<ApiResponse<AuthResponse>> RefreshTokenAsync(RefreshTokenRequest request);
    Task<ApiResponse<object>> LogoutAsync(Guid userId, string accessToken);
}
