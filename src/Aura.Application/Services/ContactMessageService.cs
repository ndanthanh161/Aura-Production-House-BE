using Aura.Application.DTOs.Contact;
using Aura.Application.Interfaces;
using Aura.Application.Mappers;
using Aura.Domain.Interfaces;
using Aura.Domain.Entity;

namespace Aura.Application.Services;

public class ContactMessageService : IContactMessageService
{
    private readonly IContactMessageRepository _repository;

    public ContactMessageService(IContactMessageRepository repository)
    {
        _repository = repository;
    }

    public async Task<ContactMessageResponseDTO> SendMessageAsync(ContactMessageRequestDTO request)
    {
        var message = ContactMapper.ToEntity(request);
        var created = await _repository.CreateAsync(message);
        return ContactMapper.ToDTO(created);
    }

    public async Task<IEnumerable<ContactMessageResponseDTO>> GetAllMessagesAsync()
    {
        var messages = await _repository.GetAllAsync();
        return messages.Select(ContactMapper.ToDTO);
    }

    public async Task<ContactMessageResponseDTO?> GetMessageByIdAsync(Guid id)
    {
        var message = await _repository.GetByIdAsync(id);
        return message != null ? ContactMapper.ToDTO(message) : null;
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
}
