using OcrApp.Server.Db;

namespace OcrApp.Server
{
    public static class ApiKeyAuthenticationExtensions
    {
        public static IServiceCollection AddApiKeyAuthentication(this IServiceCollection services)
        {
            services.AddScoped<IApiKeyService, ApiKeyService>();
            return services;
        }
    }
}