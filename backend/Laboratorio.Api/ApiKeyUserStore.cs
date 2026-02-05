using Microsoft.AspNetCore.Identity;

namespace Laboratorio.Api;

public sealed class ApiKeyUserStore
{
    private readonly PasswordHasher<string> _hasher = new();
    private readonly Dictionary<string, string> _users = new(StringComparer.OrdinalIgnoreCase);

    public ApiKeyUserStore()
    {
        var user = "demo";
        _users[user] = _hasher.HashPassword(user, "P@ssw0rd!");
    }

    public bool Validate(string username, string password)
    {
        if (!_users.TryGetValue(username, out var hash))
        {
            return false;
        }

        var result = _hasher.VerifyHashedPassword(username, hash, password);
        return result == PasswordVerificationResult.Success;
    }
}
