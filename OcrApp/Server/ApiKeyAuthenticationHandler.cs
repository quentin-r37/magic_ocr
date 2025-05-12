using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using OcrApp.Server.Db;

namespace OcrApp.Server;

public class ApiKeyAuthenticationHandler(
    IOptionsMonitor<ApiKeyAuthenticationOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder,
    IApiKeyService apiKeyService)
    : AuthenticationHandler<ApiKeyAuthenticationOptions>(options, logger, encoder)
{
    private const string ApiKeyHeaderName = "X-API-Key";

    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.TryGetValue(ApiKeyHeaderName, out var apiKeyHeaderValues))
        {
            return AuthenticateResult.NoResult();
        }

        var providedApiKey = apiKeyHeaderValues.FirstOrDefault();

        if (string.IsNullOrWhiteSpace(providedApiKey))
        {
            return AuthenticateResult.NoResult();
        }

        var apiKey = await apiKeyService.ValidateApiKeyAsync(providedApiKey);

        if (apiKey == null)
        {
            return AuthenticateResult.Fail("Invalid API Key");
        }

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, apiKey.UserId),
            new Claim(ClaimTypes.Name, apiKey.UserId),
            new Claim("ApiKeyId", apiKey.Id)
        };

        var identity = new ClaimsIdentity(claims, Options.AuthenticationType);
        var identities = new List<ClaimsIdentity> { identity };
        var principal = new ClaimsPrincipal(identities);
        var ticket = new AuthenticationTicket(principal, Options.Scheme);

        return AuthenticateResult.Success(ticket);
    }
}