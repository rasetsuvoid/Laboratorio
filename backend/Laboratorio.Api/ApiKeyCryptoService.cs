using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace Laboratorio.Api;

public sealed class ApiKeyCryptoService
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly byte[] _key;
    private readonly byte[] _aad;

    public ApiKeyCryptoService(ApiKeyCryptoOptions options)
    {
        if (string.IsNullOrWhiteSpace(options.Key))
        {
            throw new InvalidOperationException("ApiKeyCrypto:Key is required.");
        }

        _key = Convert.FromBase64String(options.Key);
        if (_key.Length != 32)
        {
            throw new InvalidOperationException("ApiKeyCrypto:Key must be 32 bytes (Base64 encoded).");
        }

        _aad = string.IsNullOrWhiteSpace(options.Aad)
            ? Array.Empty<byte>()
            : Encoding.UTF8.GetBytes(options.Aad);
    }

    public bool TryDecrypt(string apiKey, out ApiKeyPayload payload)
    {
        payload = default!;

        if (string.IsNullOrWhiteSpace(apiKey))
        {
            return false;
        }

        byte[] data;
        try
        {
            data = Convert.FromBase64String(apiKey);
        }
        catch (FormatException)
        {
            return false;
        }

        if (data.Length <= 12 + 16)
        {
            return false;
        }

        var iv = data[..12];
        var tag = data[^16..];
        var cipher = data[12..^16];
        var plaintext = new byte[cipher.Length];

        try
        {
            using var aes = new AesGcm(_key, 16);
            aes.Decrypt(iv, cipher, tag, plaintext, _aad);
        }
        catch (CryptographicException)
        {
            return false;
        }

        ApiKeyPayload? decoded;
        try
        {
            var json = Encoding.UTF8.GetString(plaintext);
            decoded = JsonSerializer.Deserialize<ApiKeyPayload>(json, JsonOptions);
        }
        catch (JsonException)
        {
            return false;
        }

        if (decoded is null || string.IsNullOrWhiteSpace(decoded.User) || string.IsNullOrWhiteSpace(decoded.Password))
        {
            return false;
        }

        payload = decoded;
        return true;
    }
}
