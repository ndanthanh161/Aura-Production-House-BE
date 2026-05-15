using Aura.Application.Interfaces;
using BCrypt.Net;

namespace Aura.Infrastructure.Services;

public class PasswordHasher : IPasswordHasher
{
    public string HashPassword(string password) => BCrypt.Net.BCrypt.HashPassword(password);
    
    public bool VerifyPassword(string password, string hashedPassword) 
        => BCrypt.Net.BCrypt.Verify(password, hashedPassword);
}
