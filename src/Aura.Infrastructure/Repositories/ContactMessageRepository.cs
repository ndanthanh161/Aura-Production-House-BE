using Aura.Domain.Interfaces;
using Aura.Domain.Entity;
using Aura.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Aura.Infrastructure.Repositories
{
    public class ContactMessageRepository : IContactMessageRepository
    {
        private readonly AppDbContext _context;

        public ContactMessageRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<ContactMessage> CreateAsync(ContactMessage message)
        {
            _context.ContactMessages.Add(message);
            await _context.SaveChangesAsync();
            return message;
        }

        public async Task<ContactMessage?> GetByIdAsync(Guid id)
        {
            return await _context.ContactMessages.FindAsync(id);
        }

        public async Task<IEnumerable<ContactMessage>> GetAllAsync()
        {
            return await _context.ContactMessages
                .OrderByDescending(m => m.CreatedAt)
                .ToListAsync();
        }

        public async Task<bool> UpdateAsync(ContactMessage message)
        {
            _context.ContactMessages.Update(message);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var message = await _context.ContactMessages.FindAsync(id);
            if (message == null) return false;

            _context.ContactMessages.Remove(message);
            return await _context.SaveChangesAsync() > 0;
        }
    }
}
