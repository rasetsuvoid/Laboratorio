using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Laboratorio.Api.Controllers;

[ApiController]
[Route("api")]
[Authorize(Policy = "Session")]
public sealed class SecureController : ControllerBase
{
    [HttpGet("secure-data")]
    public IActionResult GetSecureData()
    {
        var user = User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue(JwtRegisteredClaimNames.Sub)
            ?? "unknown";

        return Ok(new
        {
            message = "Acceso autorizado",
            user,
            issuedAt = DateTimeOffset.UtcNow
        });
    }

    [HttpGet("secure-profile")]
    public IActionResult GetSecureProfile()
    {
        var user = User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue(JwtRegisteredClaimNames.Sub)
            ?? "unknown";

        return Ok(new
        {
            user,
            roles = new[] { "lab-user" },
            environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Unknown"
        });
    }
}
