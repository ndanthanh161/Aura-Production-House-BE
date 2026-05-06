using Aura.Domain.Entity;

namespace Aura.Domain.Interfaces
{
    public interface IContactMessageRepository
    {
        Task<ContactMessage> CreateAsync(ContactMessage message);
        Task<ContactMessage?> GetByIdAsync(Guid id);
        Task<IEnumerable<ContactMessage>> GetAllAsync();
        Task<bool> UpdateAsync(ContactMessage message);
        Task<bool> DeleteAsync(Guid id);
    }
}
