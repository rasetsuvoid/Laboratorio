param(
  [string]$User = "demo",
  [string]$Password = "P@ssw0rd!"
)

$apiKey = dotnet run --project "$PSScriptRoot\..\tools\ApiKeyGenerator" -- $User $Password
$payload = @{ apiKey = $apiKey } | ConvertTo-Json -Compress
Write-Output $payload
