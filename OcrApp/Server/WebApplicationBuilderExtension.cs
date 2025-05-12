using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Identity.Web;

namespace OcrApp.Server
{
    public static class WebApplicationBuilderExtension
    {
        public static WebApplicationBuilder ConfigureAuthenticationSchemes(this WebApplicationBuilder webApplicationBuilder)
        {
            var authBuilder = webApplicationBuilder.Services.AddAuthentication(options =>
            {
                options.DefaultScheme = "MultiAuth";
                options.DefaultAuthenticateScheme = "MultiAuth";
            });

            authBuilder.AddMicrosoftIdentityWebApi(webApplicationBuilder.Configuration.GetSection("AzureAd"));
            authBuilder.AddScheme<ApiKeyAuthenticationOptions, ApiKeyAuthenticationHandler>(
                ApiKeyAuthenticationOptions.DefaultScheme,
                options => { });

            authBuilder.AddPolicyScheme("MultiAuth", "JWT or ApiKey", options =>
            {
                options.ForwardDefaultSelector = context =>
                {
                    if (context.Request.Headers.TryGetValue("X-API-Key", out var apiKeyHeaderValues) &&
                        !string.IsNullOrEmpty(apiKeyHeaderValues.FirstOrDefault()))
                    {
                        return ApiKeyAuthenticationOptions.DefaultScheme;
                    }
                    return JwtBearerDefaults.AuthenticationScheme;
                };
            });
            authBuilder.Services.AddAuthorizationBuilder().AddPolicy("JwtOnly", policy =>
            {
                policy
                    .AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme)
                    .RequireAuthenticatedUser();
            }).AddPolicy("ApiOrJwt", policy =>
            {
                policy
                    .AddAuthenticationSchemes(ApiKeyAuthenticationOptions.DefaultScheme, JwtBearerDefaults.AuthenticationScheme)
                    .RequireAuthenticatedUser();
            });

            return webApplicationBuilder;
        }
    }
}
