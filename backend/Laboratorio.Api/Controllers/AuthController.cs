using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Laboratorio.Api.Controllers;

[ApiController]
[Route("auth")]
public sealed class AuthController : ControllerBase
{
    private readonly ApiKeyUserStore _users;
    private readonly SessionStore _sessions;
    private readonly JwtTokenService _tokenService;
    private readonly ApiKeyCryptoService _crypto;
    private readonly SecurityOptions _security;
    private readonly IWebHostEnvironment _environment;

    public AuthController(
        ApiKeyUserStore users,
        SessionStore sessions,
        JwtTokenService tokenService,
        ApiKeyCryptoService crypto,
        SecurityOptions security,
        IWebHostEnvironment environment)
    {
        _users = users;
        _sessions = sessions;
        _tokenService = tokenService;
        _crypto = crypto;
        _security = security;
        _environment = environment;
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public IActionResult Login([FromBody] LoginRequest request)
    {
        if (request is null || string.IsNullOrWhiteSpace(request.ApiKey))
        {
            return BadRequest(new { error = "ApiKey is required." });
        }

        if (!_crypto.TryDecrypt(request.ApiKey, out var payload))
        {
            return Unauthorized();
        }

        if (!_users.Validate(payload.User, payload.Password))
        {
            return Unauthorized();
        }

        var (token, expiresAt) = _tokenService.Create(payload.User);
        var sessionId = _sessions.Create(payload.User, TimeSpan.FromMinutes(_security.SessionMinutes), out var csrfToken);

        var secureCookie = !_environment.IsDevelopment();

        Response.Cookies.Append(_security.JwtCookieName, token, new CookieOptions
        {
            HttpOnly = true,
            Secure = secureCookie,
            SameSite = SameSiteMode.Strict,
            Path = "/"
        });

        Response.Cookies.Append(_security.SessionCookieName, sessionId, new CookieOptions
        {
            HttpOnly = true,
            Secure = secureCookie,
            SameSite = SameSiteMode.Strict,
            Path = "/"
        });

        Response.Cookies.Append(_security.XsrfCookieName, csrfToken, new CookieOptions
        {
            HttpOnly = false,
            Secure = secureCookie,
            SameSite = SameSiteMode.Strict,
            Path = "/"
        });

        Response.Headers.CacheControl = "no-store";
        return Ok(new LoginResponse(expiresAt, payload.User));
    }

    [HttpPost("logout")]
    [Authorize(Policy = "Session")]
    public IActionResult Logout()
    {
        if (Request.Cookies.TryGetValue(_security.SessionCookieName, out var sessionId))
        {
            _sessions.Remove(sessionId);
            Response.Cookies.Delete(_security.SessionCookieName, new CookieOptions { Path = "/" });
        }

        Response.Cookies.Delete(_security.JwtCookieName, new CookieOptions { Path = "/" });
        Response.Cookies.Delete(_security.XsrfCookieName, new CookieOptions { Path = "/" });
        return NoContent();
    }
}
