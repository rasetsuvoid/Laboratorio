using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Laboratorio.Api.Security;

public sealed class SessionRequirement : IAuthorizationRequirement
{
}

public sealed class SessionHandler : AuthorizationHandler<SessionRequirement>
{
    private readonly SecurityOptions _security;
    private readonly SessionStore _sessions;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public SessionHandler(SecurityOptions security, SessionStore sessions, IHttpContextAccessor httpContextAccessor)
    {
        _security = security;
        _sessions = sessions;
        _httpContextAccessor = httpContextAccessor;
    }

    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, SessionRequirement requirement)
    {
        var httpContext = context.Resource switch
        {
            HttpContext http => http,
            AuthorizationFilterContext filter => filter.HttpContext,
            _ => _httpContextAccessor.HttpContext
        };

        if (httpContext is null)
        {
            return Task.CompletedTask;
        }

        if (httpContext.User?.Identity?.IsAuthenticated != true)
        {
            return Task.CompletedTask;
        }

        if (!httpContext.Request.Cookies.TryGetValue(_security.SessionCookieName, out var sessionId))
        {
            return Task.CompletedTask;
        }

        var username = httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? httpContext.User.FindFirstValue(JwtRegisteredClaimNames.Sub);

        if (string.IsNullOrWhiteSpace(username))
        {
            return Task.CompletedTask;
        }

        var csrfHeader = httpContext.Request.Headers[_security.XsrfHeaderName].ToString();
        if (!_sessions.Validate(sessionId, username, csrfHeader))
        {
            return Task.CompletedTask;
        }

        context.Succeed(requirement);
        return Task.CompletedTask;
    }
}
