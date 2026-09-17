using System;
using Microsoft.AspNetCore.Identity;
using TaskService.Models;

namespace TaskService.Utils;

public class Utils
{
    private PasswordHasher<User> _hasher = new();
    public string GetPasswordHash(string password, User user)
    {
        return _hasher.HashPassword(user, password);
    }
        
    public bool CheckPassword(string password, string hash, User user)
    {
        return _hasher.VerifyHashedPassword(user, hash, password) == PasswordVerificationResult.Success;
    }
}
