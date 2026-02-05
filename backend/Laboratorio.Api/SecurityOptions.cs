namespace Laboratorio.Api;

public sealed class SecurityOptions
{
    public string SessionCookieName { get; init; } = "lab_session";
    public string JwtCookieName { get; init; } = "lab_jwt";
    public string XsrfCookieName { get; init; } = "XSRF-TOKEN";
    public string XsrfHeaderName { get; init; } = "X-XSRF-TOKEN";
    public int SessionMinutes { get; init; } = 60;
}
