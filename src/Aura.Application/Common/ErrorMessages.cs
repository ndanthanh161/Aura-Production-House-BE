namespace Aura.Application.Common
{
    public static class ErrorMessages
    {
        // ── Auth ──────────────────────────────────────────
        public const string InvalidEmailOrPassword = "Invalid email or password.";
        public const string AccountDeactivated = "Your account has been deactivated. Please contact support.";
        public const string DuplicateEmail = "Email is already registered.";
        public const string InvalidGoogleToken = "Invalid Google token.";
        public const string InvalidRefreshToken = "Invalid refresh token.";
        public const string ExpiredRefreshToken = "Refresh token has expired. Please login again.";
        public const string InvalidOrExpiredOtp = "Invalid or expired OTP.";

        // ── User / Resource ──────────────────────────────
        public const string UserNotFound = "User not found.";
        public const string ResourceNotFound = "Resource not found.";
        public const string PortfolioItemNotFound = "Portfolio item not found.";
        public const string MediaNotFound = "Media not found.";
        public const string MessageNotFound = "Message not found.";
        public const string ProjectNotFound = "Project not found.";
        public const string PackageNotFound = "Package not found.";
        
        // ── Infrastructure ────────────────────────────────
        public const string CloudinaryUploadFailed = "Cloudinary upload failed: {0}";
        public const string FileIsEmpty = "File is empty.";
    }
}
