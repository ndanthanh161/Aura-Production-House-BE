using Aura.Application.DTOs.User;
using Aura.Application.DTOs.Auth;
using Aura.Domain.Entity;
using Google.Apis.Auth;

namespace Aura.Application.Mappers;

public static class UserMapper
{
    public static UserResponseDTO ToDTO(User user)
    {
        return new UserResponseDTO
        {
            Id = user.Id,
            FullName = user.FullName,
            Email = user.Email,
            Phone = user.Phone,
            Avatar = user.Avatar,
            Bio = user.Bio,
            Specialization = user.Specialization,
            Role = user.Role?.Name ?? string.Empty,
            IsActive = user.IsActive,
            CreatedAt = user.CreatedAt,
            UpdatedAt = user.UpdatedAt
        };
    }

    public static User ToEntity(CreatePhotographerRequestDTO request, Guid roleId, string hashedPassword)
    {
        return new User
        {
            Id = Guid.NewGuid(),
            FullName = request.FullName,
            Email = request.Email.ToLower().Trim(),
            Password = hashedPassword,
            Phone = request.Phone,
            RoleId = roleId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    public static User ToEntityFromRegister(RegisterRequest request, Guid roleId, string hashedPassword)
    {
        return new User
        {
            Id = Guid.NewGuid(),
            FullName = request.FullName,
            Email = request.Email.ToLower().Trim(),
            Password = hashedPassword,
            Phone = request.Phone,
            RoleId = roleId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    public static User ToEntityFromGoogle(GoogleJsonWebSignature.Payload payload, string email, Guid roleId, string randomHashedPassword)
    {
        return new User
        {
            Id = Guid.NewGuid(),
            FullName = payload.Name ?? payload.Email,
            Email = email,
            Password = randomHashedPassword,
            Avatar = payload.Picture,
            RoleId = roleId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    public static void UpdateUser(User user, UpdateUserRequestDTO request)
    {
        user.FullName = request.FullName;
        user.Phone = request.Phone;
        user.Avatar = request.Avatar;
        user.Bio = request.Bio;
        user.Specialization = request.Specialization;
        user.UpdatedAt = DateTime.UtcNow;
    }
}
