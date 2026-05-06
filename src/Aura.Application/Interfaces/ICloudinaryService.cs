using Microsoft.AspNetCore.Http;

namespace Aura.Application.Interfaces
{
    public interface ICloudinaryService
    {
        /// <summary>
        /// Upload file (image or video) to Cloudinary
        /// </summary>
        /// <returns>(url, publicId)</returns>
        Task<(string Url, string PublicId)> UploadAsync(IFormFile file, string folder = "portfolio");

        /// <summary>
        /// Delete file from Cloudinary by publicId
        /// </summary>
        Task<bool> DeleteAsync(string publicId);

        /// <summary>
        /// Generate a signature for client-side upload
        /// </summary>
        string GenerateSignature(IDictionary<string, object> parameters);

        /// <summary>
        /// Get Cloudinary settings (CloudName, ApiKey) for client-side use
        /// </summary>
        (string CloudName, string ApiKey) GetCloudSettings();
    }
}
