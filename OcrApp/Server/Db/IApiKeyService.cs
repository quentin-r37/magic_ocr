namespace OcrApp.Server.Db;

public interface IApiKeyService
{
    Task<ApiKey> GenerateApiKeyForUserAsync(string userId, string name, DateTime? expiresAt = null);
    Task<ApiKey?> ValidateApiKeyAsync(string apiKey);
    Task<IEnumerable<ApiKey>> GetApiKeysForUserAsync(string userId);
    Task<bool> RevokeApiKeyAsync(string apiKeyId);
}