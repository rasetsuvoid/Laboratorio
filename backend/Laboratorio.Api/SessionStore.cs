using System.Collections.Concurrent;
using System.Security.Cryptography;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace Laboratorio.Api;

public sealed class SessionStore
{
    private readonly ConcurrentDictionary<string, SessionInfo> _sessions = new();

    public string Create(string username, TimeSpan ttl, out string csrfToken)
    {
        var sessionId = GenerateToken();
        csrfToken = GenerateToken();
        _sessions[sessionId] = new SessionInfo(username, csrfToken, DateTimeOffset.UtcNow.Add(ttl));
        return sessionId;
    }

    public bool Validate(string sessionId, string username, string csrfToken)
    {
        if (!_sessions.TryGetValue(sessionId, out var info))
        {
            return false;
        }

        if (info.ExpiresAt <= DateTimeOffset.UtcNow)
        {
            _sessions.TryRemove(sessionId, out _);
            return false;
        }

        if (!string.Equals(info.Username, username, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        if (string.IsNullOrWhiteSpace(csrfToken) || csrfToken.Length != info.CsrfToken.Length)
        {
            return false;
        }

        var left = Encoding.UTF8.GetBytes(info.CsrfToken);
        var right = Encoding.UTF8.GetBytes(csrfToken);
        return CryptographicOperations.FixedTimeEquals(left, right);
    }

    public void Remove(string sessionId) => _sessions.TryRemove(sessionId, out _);

    private static string GenerateToken()
    {
        var bytes = new byte[32];
        RandomNumberGenerator.Fill(bytes);
        return Base64UrlEncoder.Encode(bytes);
    }

    private sealed record SessionInfo(string Username, string CsrfToken, DateTimeOffset ExpiresAt);
}
