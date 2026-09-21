using Microsoft.AspNetCore.Identity;
using TaskService.Models;

namespace TaskService.Utils;

public static class PasswordHashVer
{
    static private PasswordHasher<User> _hasher = new();
    static public string Hash(string password, User user)
    {
        return _hasher.HashPassword(user, password);
    }
    
    static public bool Verify(string password, string hash, User user)
    {
        return _hasher.VerifyHashedPassword(user, hash, password) == PasswordVerificationResult.Success;
    }
}
