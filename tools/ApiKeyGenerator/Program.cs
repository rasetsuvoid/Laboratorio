using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Encodings.Web;

var user = args.Length > 0 ? args[0] : "demo";
var password = args.Length > 1 ? args[1] : "P@ssw0rd!";

var root = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", ".."));
var settingsPath = Path.Combine(root, "backend", "Laboratorio.Api", "appsettings.json");

if (!File.Exists(settingsPath))
{
    Console.Error.WriteLine($"No se encontró appsettings.json en {settingsPath}");
    return;
}

using var settingsStream = File.OpenRead(settingsPath);
using var document = JsonDocument.Parse(settingsStream);

if (!document.RootElement.TryGetProperty("ApiKeyCrypto", out var apiKeyCrypto) ||
    !apiKeyCrypto.TryGetProperty("Key", out var keyElement))
{
    Console.Error.WriteLine("ApiKeyCrypto:Key no encontrado en appsettings.json");
    return;
}

var keyBase64 = keyElement.GetString();
if (string.IsNullOrWhiteSpace(keyBase64))
{
    Console.Error.WriteLine("ApiKeyCrypto:Key vacío.");
    return;
}

var aad = apiKeyCrypto.TryGetProperty("Aad", out var aadElement)
    ? aadElement.GetString()
    : "";

var key = Convert.FromBase64String(keyBase64);

var iv = new byte[12];
RandomNumberGenerator.Fill(iv);

var payloadJson = JsonSerializer.Serialize(new { user, password });
var plaintext = Encoding.UTF8.GetBytes(payloadJson);
var cipher = new byte[plaintext.Length];
var tag = new byte[16];

using var aes = new AesGcm(key, 16);
if (string.IsNullOrWhiteSpace(aad))
{
    aes.Encrypt(iv, plaintext, cipher, tag);
}
else
{
    var aadBytes = Encoding.UTF8.GetBytes(aad);
    aes.Encrypt(iv, plaintext, cipher, tag, aadBytes);
}

var output = new byte[iv.Length + cipher.Length + tag.Length];
Buffer.BlockCopy(iv, 0, output, 0, iv.Length);
Buffer.BlockCopy(cipher, 0, output, iv.Length, cipher.Length);
Buffer.BlockCopy(tag, 0, output, iv.Length + cipher.Length, tag.Length);

Console.WriteLine(Convert.ToBase64String(output));
