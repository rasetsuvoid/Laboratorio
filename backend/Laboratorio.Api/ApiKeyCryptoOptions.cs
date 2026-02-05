namespace Laboratorio.Api;

public sealed class ApiKeyCryptoOptions
{
    public string Key { get; init; } = string.Empty;
    public string Aad { get; init; } = "laboratorio-login-v1";
}
