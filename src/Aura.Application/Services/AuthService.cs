using Aura.Application.Common;
using Aura.Application.DTOs.Auth;
using Aura.Application.Interfaces;
using Aura.Application.Mappers;
using Aura.Domain.Entity;
using Aura.Domain.Interfaces;
using Google.Apis.Auth;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;

namespace Aura.Application.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IConfiguration _configuration;
    private readonly IRoleRepository _roleRepository;
    private readonly IMailService _mailService;
    private readonly IEmailTemplateService _templateService;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ITokenBlacklistService _tokenBlacklistService;
    private readonly IDistributedCache _cache;

    public AuthService(
        IUserRepository userRepository,
        IRefreshTokenRepository refreshTokenRepository,
        IJwtTokenService jwtTokenService,
        IConfiguration configuration,
        IRoleRepository roleRepository,
        ITokenBlacklistService tokenBlacklistService,
        IDistributedCache cache,
        IMailService mailService,
        IEmailTemplateService templateService,
        IPasswordHasher passwordHasher)
    {
        _userRepository = userRepository;
        _refreshTokenRepository = refreshTokenRepository;
        _jwtTokenService = jwtTokenService;
        _configuration = configuration;
        _roleRepository = roleRepository;
        _tokenBlacklistService = tokenBlacklistService;
        _cache = cache;
        _mailService = mailService;
        _templateService = templateService;
        _passwordHasher = passwordHasher;
    }

    public async Task<ApiResponse<AuthResponse>> RegisterAsync(RegisterRequest request)
    {
        // 1. Validate email unique
        if (await _userRepository.ExistsByEmailAsync(request.Email))
        {
            return ApiResponse<AuthResponse>.ErrorResponse(
                ErrorMessages.DuplicateEmail,
                409,
                new List<string> { "DUPLICATE_EMAIL" }
            );
        }

        // 2. Hash password
        var hashedPassword = _passwordHasher.HashPassword(request.Password);


        var roleId = await GetDefaultRoleIdAsync();
        var user = UserMapper.ToEntityFromRegister(request, roleId, hashedPassword);

        // 4. Save user
        var createdUser = await _userRepository.CreateAsync(user);

        // 5. Generate tokens
        var accessToken = _jwtTokenService.GenerateAccessToken(createdUser);
        var refreshToken = _jwtTokenService.GenerateRefreshToken();

        // 6. Save refresh token
        await SaveRefreshTokenAsync(createdUser.Id, refreshToken);

        // 7. Build response
        var response = new AuthResponse
        {
            UserId = createdUser.Id,
            FullName = createdUser.FullName,
            Email = createdUser.Email,
            Role = createdUser.Role.Name,
            IsVip = createdUser.IsVip && createdUser.VipExpireAt.HasValue && createdUser.VipExpireAt.Value > DateTime.UtcNow,
            VipExpireAt = createdUser.VipExpireAt,
            Avatar = createdUser.Avatar,
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            AccessTokenExpiresAt = _jwtTokenService.GetAccessTokenExpiration()
        };

        return ApiResponse<AuthResponse>.CreatedResponse(response, "Registration successful.");
    }

    public async Task<ApiResponse<AuthResponse>> LoginAsync(LoginRequest request)
    {
        // 1. Find user by email
        var user = await _userRepository.GetByEmailAsync(request.Email.ToLower().Trim());
        if (user == null)
        {
            return ApiResponse<AuthResponse>.UnauthorizedResponse(ErrorMessages.InvalidEmailOrPassword);
        }

        // Kiểm tra tài khoản có bị khóa không
        if (!user.IsActive)
        {
            return ApiResponse<AuthResponse>.UnauthorizedResponse(ErrorMessages.AccountDeactivated);
        }

        // 2. Verify password
        if (!_passwordHasher.VerifyPassword(request.Password, user.Password))
        {
            return ApiResponse<AuthResponse>.UnauthorizedResponse(ErrorMessages.InvalidEmailOrPassword);
        }


        // 3. Generate tokens
        var accessToken = _jwtTokenService.GenerateAccessToken(user);
        var refreshToken = _jwtTokenService.GenerateRefreshToken();

        // 4. Save refresh token (xóa token cũ trước)
        await _refreshTokenRepository.DeleteAllByUserIdAsync(user.Id);
        await SaveRefreshTokenAsync(user.Id, refreshToken);

        // 5. Build response
        var response = new AuthResponse
        {
            UserId = user.Id,
            FullName = user.FullName,
            Email = user.Email,
            Role = user.Role.Name,
            IsVip = user.IsVip && user.VipExpireAt.HasValue && user.VipExpireAt.Value > DateTime.UtcNow,
            VipExpireAt = user.VipExpireAt,
            Avatar = user.Avatar,
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            AccessTokenExpiresAt = _jwtTokenService.GetAccessTokenExpiration()
        };

        return ApiResponse<AuthResponse>.SuccessResponse(response, "Login successful.");
    }

    /// <summary>
    /// Google Login: verify Google ID token, auto-create user if first time, return JWT tokens
    /// </summary>
    public async Task<ApiResponse<AuthResponse>> GoogleLoginAsync(string googleIdToken)
    {
        // 1. Verify Google ID token
        GoogleJsonWebSignature.Payload payload;
        try
        {
            var googleClientId = _configuration["GoogleAuth:ClientId"];
            var settings = new GoogleJsonWebSignature.ValidationSettings
            {
                Audience = new[] { googleClientId }
            };
            payload = await GoogleJsonWebSignature.ValidateAsync(googleIdToken, settings);
        }
        catch (InvalidJwtException)
        {
            return ApiResponse<AuthResponse>.UnauthorizedResponse(ErrorMessages.InvalidGoogleToken);
        }

        // 2. Find or create user
        var email = payload.Email.ToLower().Trim();
        var user = await _userRepository.GetByEmailAsync(email);

        if (user == null)
        {
            user = await CreateUserFromGoogleAsync(payload, email);
        }


        // Kiểm tra tài khoản có bị khóa không
        if (!user.IsActive)
        {
            return ApiResponse<AuthResponse>.UnauthorizedResponse(ErrorMessages.AccountDeactivated);
        }

        // 3. Generate JWT tokens
        var accessToken = _jwtTokenService.GenerateAccessToken(user);
        var refreshToken = _jwtTokenService.GenerateRefreshToken();

        // 4. Save refresh token
        await _refreshTokenRepository.DeleteAllByUserIdAsync(user.Id);
        await SaveRefreshTokenAsync(user.Id, refreshToken);

        // 5. Build response
        var response = new AuthResponse
        {
            UserId = user.Id,
            FullName = user.FullName,
            Email = user.Email,
            Role = user.Role.Name,
            IsVip = user.IsVip && user.VipExpireAt.HasValue && user.VipExpireAt.Value > DateTime.UtcNow,
            VipExpireAt = user.VipExpireAt,
            Avatar = user.Avatar,
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            AccessTokenExpiresAt = _jwtTokenService.GetAccessTokenExpiration()
        };

        return ApiResponse<AuthResponse>.SuccessResponse(response, "Google login successful.");
    }

    public async Task<ApiResponse<AuthResponse>> RefreshTokenAsync(RefreshTokenRequest request)
    {
        // 1. Find refresh token
        var storedToken = await _refreshTokenRepository.GetByTokenAsync(request.RefreshToken);
        if (storedToken == null)
        {
            return ApiResponse<AuthResponse>.UnauthorizedResponse(ErrorMessages.InvalidRefreshToken);
        }

        // 2. Check if expired
        if (storedToken.ExpiresAt < DateTime.UtcNow)
        {
            await _refreshTokenRepository.DeleteAsync(storedToken);
            return ApiResponse<AuthResponse>.UnauthorizedResponse(ErrorMessages.ExpiredRefreshToken);
        }

        // 3. Delete old refresh token
        await _refreshTokenRepository.DeleteAsync(storedToken);

        // 4. Generate new tokens
        var user = storedToken.User;
        var newAccessToken = _jwtTokenService.GenerateAccessToken(user);
        var newRefreshToken = _jwtTokenService.GenerateRefreshToken();

        // 5. Save new refresh token
        await SaveRefreshTokenAsync(user.Id, newRefreshToken);

        // 6. Build response
        var response = new AuthResponse
        {
            UserId = user.Id,
            FullName = user.FullName,
            Email = user.Email,
            Role = user.Role.Name,
            IsVip = user.IsVip && user.VipExpireAt.HasValue && user.VipExpireAt.Value > DateTime.UtcNow,
            VipExpireAt = user.VipExpireAt,
            Avatar = user.Avatar,
            AccessToken = newAccessToken,
            RefreshToken = newRefreshToken,
            AccessTokenExpiresAt = _jwtTokenService.GetAccessTokenExpiration()
        };

        return ApiResponse<AuthResponse>.SuccessResponse(response, "Token refreshed successfully.");
    }

    public async Task<ApiResponse<object>> LogoutAsync(Guid userId, string accessToken)
    {
        // 1. Xóa tất cả refresh tokens
        await _refreshTokenRepository.DeleteAllByUserIdAsync(userId);

        // 2. Đọc jti và expiration từ access token hiện tại
        var handler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(accessToken);

        var jti = jwtToken.Id; // Lấy jti claim
        var expiration = jwtToken.ValidTo - DateTime.UtcNow; // Thời gian còn lại

        // 3. Blacklist token trong Redis (tự xóa khi hết hạn)
        if (expiration > TimeSpan.Zero)
        {
            await _tokenBlacklistService.BlacklistTokenAsync(jti, expiration);
        }

        return ApiResponse<object>.SuccessResponse(null!, "Logout successful.");
    }

    public async Task<ApiResponse<object>> ForgotPasswordAsync(ForgotPasswordRequest request)
    {
        var user = await _userRepository.GetByEmailAsync(request.Email.ToLower().Trim());
        if (user == null)
        {
            // For security, don't reveal if email exists, but here the user asked to "check email tồn tại"
            return ApiResponse<object>.ErrorResponse("Email does not exist in our system.", 404);
        }

        // Generate 6-digit OTP
        var otp = new Random().Next(100000, 999999).ToString();

        // Store in cache (5 minutes)
        var cacheKey = $"otp:{user.Email}";
        await _cache.SetStringAsync(cacheKey, otp, new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
        });

        // Send email
        var subject = "Aura Production House - Password Reset OTP";
        var body = _templateService.GetOtpEmailTemplate(user.FullName, otp);

        await _mailService.SendEmailAsync(user.Email, subject, body);

        return ApiResponse<object>.SuccessResponse(null!, "OTP has been sent to your email.");
    }

    public async Task<ApiResponse<object>> ResetPasswordAsync(ResetPasswordRequest request)
    {
        var cacheKey = $"otp:{request.Email.ToLower().Trim()}";
        var storedOtp = await _cache.GetStringAsync(cacheKey);

        if (storedOtp == null || storedOtp != request.Otp)
        {
            return ApiResponse<object>.ErrorResponse(ErrorMessages.InvalidOrExpiredOtp, 400);
        }

        var user = await _userRepository.GetByEmailAsync(request.Email.ToLower().Trim());
        if (user == null)
        {
            return ApiResponse<object>.ErrorResponse(ErrorMessages.UserNotFound, 404);
        }

        // Update password
        user.Password = _passwordHasher.HashPassword(request.NewPassword);
        user.UpdatedAt = DateTime.UtcNow;

        await _userRepository.UpdateAsync(user);

        // Clear OTP from cache
        await _cache.RemoveAsync(cacheKey);

        return ApiResponse<object>.SuccessResponse(null!, "Password has been reset successfully.");
    }


    // ===== Private Helpers =====

    private async Task<User> CreateUserFromGoogleAsync(GoogleJsonWebSignature.Payload payload, string email)
    {
        var randomHashedPassword = _passwordHasher.HashPassword(Guid.NewGuid().ToString());
        var roleId = await GetDefaultRoleIdAsync();
        
        var user = UserMapper.ToEntityFromGoogle(payload, email, roleId, randomHashedPassword);


        return await _userRepository.CreateAsync(user);
    }

    private async Task SaveRefreshTokenAsync(Guid userId, string token)
    {
        var refreshTokenDays = int.Parse(_configuration["JwtSettings:RefreshTokenExpirationDays"]!);

        var refreshToken = new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Token = token,
            ExpiresAt = DateTime.UtcNow.AddDays(refreshTokenDays),
            CreatedAt = DateTime.UtcNow
        };

        await _refreshTokenRepository.CreateAsync(refreshToken);
    }

    private async Task<Guid> GetDefaultRoleIdAsync()
    {
        var role = await _roleRepository.GetByNameAsync("User");
        if (role == null)
            throw new InvalidOperationException("Default role 'Customer' not found. Please run DataSeeder.");
        return role.Id;
    }
}
