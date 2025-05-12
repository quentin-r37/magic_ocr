using Microsoft.EntityFrameworkCore;

namespace OcrApp.Server.Db;

public class ApiKeyService(ApiKeyDbContext context) : IApiKeyService
{
    public async Task<ApiKey> GenerateApiKeyForUserAsync(string userId, string name, DateTime? expiresAt = null)
    {
        var apiKey = new ApiKey
        {
            UserId = userId,
            Name = name,
            ExpiresAt = expiresAt
        };
        context.ApiKeys.Add(apiKey);
        await context.SaveChangesAsync();
        return apiKey;
    }

    public async Task<ApiKey?> ValidateApiKeyAsync(string apiKey)
    {
        return await context.ApiKeys
            .FirstOrDefaultAsync(k =>
                k.Key == apiKey &&
                k.IsActive &&
                (k.ExpiresAt == null || k.ExpiresAt > DateTime.UtcNow));
    }

    public async Task<IEnumerable<ApiKey>> GetApiKeysForUserAsync(string userId)
    {
        return await context.ApiKeys
            .Where(k => k.UserId == userId)
            .ToListAsync();
    }

    public async Task<bool> RevokeApiKeyAsync(string apiKeyId)
    {
        var apiKey = await context.ApiKeys.FindAsync(apiKeyId);
        if (apiKey == null)
            return false;

        context.ApiKeys.Remove(apiKey);
        await context.SaveChangesAsync();

        return true;
    }
}