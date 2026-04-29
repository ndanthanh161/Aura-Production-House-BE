namespace Aura.Application.DTOs.Auth;

/// <summary>
/// Request DTO for Google Login - receives the ID token from Google Sign-In
/// </summary>
public class GoogleLoginRequest
{
    public string IdToken { get; set; } = string.Empty;
}
