using Aura.Application.DTOs.Contact;

namespace Aura.Application.Interfaces
{
    public interface IContactMessageService
    {
        Task<ContactMessageResponseDTO> SendMessageAsync(ContactMessageRequestDTO request);
        Task<IEnumerable<ContactMessageResponseDTO>> GetAllMessagesAsync();
        Task<ContactMessageResponseDTO?> GetMessageByIdAsync(Guid id);
        Task<bool> MarkAsReadAsync(Guid id);
        Task<bool> DeleteMessageAsync(Guid id);
    }
}
