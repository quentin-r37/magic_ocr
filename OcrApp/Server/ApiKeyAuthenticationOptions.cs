using Microsoft.AspNetCore.Authentication;

namespace OcrApp.Server;

public class ApiKeyAuthenticationOptions : AuthenticationSchemeOptions
{
    public const string DefaultScheme = "CustomApiScheme";
    public string Scheme => DefaultScheme;
    public string AuthenticationType = DefaultScheme;
}