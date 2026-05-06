using Aura.Application.DTOs.Contact;
using Aura.Application.Interfaces;
using Aura.Domain.Interfaces;
using Aura.Domain.Entity;

namespace Aura.Infrastructure.Services
{
    public class ContactMessageService : IContactMessageService
    {
        private readonly IContactMessageRepository _repository;

        public ContactMessageService(IContactMessageRepository repository)
        {
            _repository = repository;
        }

        public async Task<ContactMessageResponseDTO> SendMessageAsync(ContactMessageRequestDTO request)
        {
            var message = new ContactMessage
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

            var created = await _repository.CreateAsync(message);

            return MapToResponseDTO(created);
        }

        public async Task<IEnumerable<ContactMessageResponseDTO>> GetAllMessagesAsync()
        {
            var messages = await _repository.GetAllAsync();
            return messages.Select(MapToResponseDTO);
        }

        public async Task<ContactMessageResponseDTO?> GetMessageByIdAsync(Guid id)
        {
            var message = await _repository.GetByIdAsync(id);
            return message != null ? MapToResponseDTO(message) : null;
        }

        public async Task<bool> MarkAsReadAsync(Guid id)
        {
            var message = await _repository.GetByIdAsync(id);
            if (message == null) return false;

            message.IsRead = true;
            return await _repository.UpdateAsync(message);
        }

        public async Task<bool> DeleteMessageAsync(Guid id)
        {
            return await _repository.DeleteAsync(id);
        }

        private ContactMessageResponseDTO MapToResponseDTO(ContactMessage message)
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
}
