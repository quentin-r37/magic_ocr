using OcrApp.Server.Models;
using OcrApp.Shared;
using Refit;

namespace OcrApp.Client.Services;

public interface IApiKeyService
{
    [Get("/api/keys")]
    Task<List<ApiKeyModel>> GetApiKeysAsync();

    [Post("/api/keys")]
    Task<ApiKeyModel> CreateApiKeyAsync([Body] ApiKeyCreateModel model);

    [Delete("/api/keys/{id}")]
    Task RevokeApiKeyAsync(string id);
}