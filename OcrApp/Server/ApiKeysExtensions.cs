using System.Security.Claims;
using OcrApp.Server.Db;
using OcrApp.Server.Models;

namespace OcrApp.Server;

public static class ApiKeysExtensions
{
    public static WebApplication AddApiKeysApi(this WebApplication app)
    {
        app.MapGet("/api/keys", async (HttpContext context, IApiKeyService apiKeyService) =>
        {
            var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
            var keys = await apiKeyService.GetApiKeysForUserAsync(userId);
            return Results.Ok(keys);
        })
        .WithTags("Keys")
        .RequireAuthorization("JwtOnly");

        app.MapPost("/api/keys", async (ApiKeyCreateModel model, HttpContext context, IApiKeyService apiKeyService) =>
        {
            var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
            var apiKey = await apiKeyService.GenerateApiKeyForUserAsync(userId, model.Name, model.ExpiresAt);
            return Results.Created($"/api/keys", apiKey);
        })
        .WithTags("Keys")
        .RequireAuthorization("JwtOnly");

        app.MapDelete("/api/keys/{id}",  async (string id, HttpContext context, IApiKeyService apiKeyService) =>
        {
            var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
            var userKeys = await apiKeyService.GetApiKeysForUserAsync(userId);
            if (userKeys.All(k => k.Id != id))
            {
                return Results.NotFound();
            }
            var result = await apiKeyService.RevokeApiKeyAsync(id);
            return result ? Results.NoContent() : Results.NotFound();
        })
        .WithTags("Keys")
        .RequireAuthorization("JwtOnly");

        return app;
    }
}