using Aura.Application.Common;
using Aura.Application.Interfaces;
using Aura.Domain.Settings;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace Aura.Infrastructure.Services
{
    public class CloudinaryService : ICloudinaryService
    {
        private readonly Cloudinary _cloudinary;

        public CloudinaryService(IOptions<CloudinarySettings> settings)
        {
            var account = new Account(
                settings.Value.CloudName,
                settings.Value.ApiKey,
                settings.Value.ApiSecret
            );
            _cloudinary = new Cloudinary(account);
            _cloudinary.Api.Timeout = 600000; // 10 minutes in milliseconds
        }

        public async Task<(string Url, string PublicId)> UploadAsync(IFormFile file, string folder = "portfolio")
        {
            if (file.Length == 0)
                throw new ArgumentException(ErrorMessages.FileIsEmpty);

            using var stream = file.OpenReadStream();
            
            var extension = System.IO.Path.GetExtension(file.FileName).ToLower();
            var isRaw = extension == ".pdf" || extension == ".docx" || extension == ".doc" ||
                        extension == ".xls" || extension == ".xlsx" || extension == ".csv" ||
                        extension == ".ods";
            var isVideo = file.ContentType.StartsWith("video/");

            if (isRaw)
            {
                var uploadParams = new RawUploadParams
                {
                    File = new FileDescription(file.FileName, stream),
                    Folder = $"aura/{folder}"
                };

                var result = await _cloudinary.UploadAsync(uploadParams);
                if (result.Error != null)
                    throw new Exception(string.Format(ErrorMessages.CloudinaryUploadFailed, result.Error.Message));

                return (result.SecureUrl.ToString(), result.PublicId);
            }
            else if (isVideo)
            {
                var uploadParams = new VideoUploadParams
                {
                    File = new FileDescription(file.FileName, stream),
                    Folder = $"aura/{folder}",
                    Transformation = new Transformation().Quality("auto")
                };

                var result = await _cloudinary.UploadAsync(uploadParams);
                if (result.Error != null)
                    throw new Exception(string.Format(ErrorMessages.CloudinaryUploadFailed, result.Error.Message));

                return (result.SecureUrl.ToString(), result.PublicId);
            }
            else
            {
                var uploadParams = new ImageUploadParams
                {
                    File = new FileDescription(file.FileName, stream),
                    Folder = $"aura/{folder}",
                    Transformation = new Transformation().Quality("auto").FetchFormat("auto")
                };

                var result = await _cloudinary.UploadAsync(uploadParams);
                if (result.Error != null)
                    throw new Exception(string.Format(ErrorMessages.CloudinaryUploadFailed, result.Error.Message));

                return (result.SecureUrl.ToString(), result.PublicId);
            }
        }

        public async Task<bool> DeleteAsync(string publicId, string resourceType = "image")
        {
            var deleteParams = new DeletionParams(publicId)
            {
                ResourceType = resourceType.ToLower() == "raw" ? ResourceType.Raw : (resourceType.ToLower() == "video" ? ResourceType.Video : ResourceType.Image)
            };
            var result = await _cloudinary.DestroyAsync(deleteParams);
            return result.Result == "ok";
        }
        
        public string GenerateSignature(IDictionary<string, object> parameters)
        {
            return _cloudinary.Api.SignParameters(parameters);
        }

        public (string CloudName, string ApiKey) GetCloudSettings()
        {
            return (_cloudinary.Api.Account.Cloud, _cloudinary.Api.Account.ApiKey);
        }
    }
}
