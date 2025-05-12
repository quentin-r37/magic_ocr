using OcrApp.Shared;

namespace OcrApp.Client.Redux.Actions;

public class ApiKeyActions
{
    // Load API Keys
    public record LoadApiKeysAction();
    public record LoadApiKeysSuccessAction(List<ApiKeyModel> ApiKeys);
    public record LoadApiKeysFailureAction(string ErrorMessage);

    // Create API Key
    public record CreateApiKeyAction(string Name, DateTime? ExpiresAt);
    public record CreateApiKeySuccessAction(ApiKeyModel ApiKey);
    public record CreateApiKeyFailureAction(string ErrorMessage);
    public record ClearNewKeyValueAction();

    // Revoke API Key
    public record RevokeApiKeyAction(string Id);
    public record RevokeApiKeySuccessAction(string Id);
    public record RevokeApiKeyFailureAction(string ErrorMessage);
}