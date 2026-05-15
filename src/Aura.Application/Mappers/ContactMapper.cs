using Aura.Application.DTOs.Contact;
using Aura.Domain.Entity;

namespace Aura.Application.Mappers;

public static class ContactMapper
{
    public static ContactMessage ToEntity(ContactMessageRequestDTO request)
    {
        return new ContactMessage
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Email = request.Email,
            PhoneNumber = request.PhoneNumber,
            Subject = request.Subject,
            Message = request.Message,
            CreatedAt = DateTime.UtcNow,
            IsRead = false
        };
    }

    public static ContactMessageResponseDTO ToDTO(ContactMessage message)
    {
        return new ContactMessageResponseDTO
        {
            Id = message.Id,
            Name = message.Name,
            Email = message.Email,
            PhoneNumber = message.PhoneNumber,
            Subject = message.Subject,
            Message = message.Message,
            IsRead = message.IsRead,
            CreatedAt = message.CreatedAt
        };
    }
}
