namespace Laboratorio.Api;

public sealed record LoginRequest(string ApiKey);
public sealed record LoginResponse(DateTimeOffset ExpiresAt, string User);
public sealed record ApiKeyPayload(string User, string Password);
