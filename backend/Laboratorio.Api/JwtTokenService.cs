using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace Laboratorio.Api;

public sealed class JwtTokenService
{
    private readonly JwtOptions _options;
    private readonly SigningCredentials _credentials;

    public JwtTokenService(JwtOptions options)
    {
        _options = options;
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(options.SigningKey));
        _credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
    }

    public (string Token, DateTimeOffset ExpiresAt) Create(string username)
    {
        var now = DateTimeOffset.UtcNow;
        var expires = now.AddMinutes(_options.TokenMinutes <= 0 ? 15 : _options.TokenMinutes);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, username),
            new Claim(JwtRegisteredClaimNames.Sub, username),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var token = new JwtSecurityToken(
            issuer: _options.Issuer,
            audience: _options.Audience,
            claims: claims,
            notBefore: now.UtcDateTime,
            expires: expires.UtcDateTime,
            signingCredentials: _credentials);

        var handler = new JwtSecurityTokenHandler();
        return (handler.WriteToken(token), expires);
    }
}
