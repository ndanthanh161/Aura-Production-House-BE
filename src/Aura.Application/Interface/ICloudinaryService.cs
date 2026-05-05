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
    }
}
