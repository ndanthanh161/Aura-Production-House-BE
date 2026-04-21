namespace Aura.Application.Interfaces;

public interface ITokenBlacklistService
{
    /// <summary>
    /// Thêm token vào blacklist (khi logout)
    /// </summary>
    /// <param name="jti">JWT ID của access token</param>
    /// <param name="expiration">Thời gian còn lại cho đến khi token hết hạn</param>
    Task BlacklistTokenAsync(string jti, TimeSpan expiration);

    /// <summary>
    /// Kiểm tra token có bị blacklist không
    /// </summary>
    /// <param name="jti">JWT ID của access token</param>
    /// <returns>True nếu token đã bị blacklist</returns>
    Task<bool> IsTokenBlacklistedAsync(string jti);
}
