using Fluxor;
using OcrApp.Client.Redux.Actions;
using OcrApp.Client.Services;
using OcrApp.Server.Models;

namespace OcrApp.Client.Redux.Effects;

public class ApiKeyEffects(IApiKeyService apiKeyService)
{
    [EffectMethod]
    public async Task HandleLoadApiKeysAction(ApiKeyActions.LoadApiKeysAction action, IDispatcher dispatcher)
    {
        try
        {
            var apiKeys = await apiKeyService.GetApiKeysAsync();
            dispatcher.Dispatch(new ApiKeyActions.LoadApiKeysSuccessAction(apiKeys));
        }
        catch (Exception ex)
        {
            dispatcher.Dispatch(new ApiKeyActions.LoadApiKeysFailureAction(ex.Message));
        }
    }

    [EffectMethod]
    public async Task HandleCreateApiKeyAction(ApiKeyActions.CreateApiKeyAction action, IDispatcher dispatcher)
    {
        try
        {
            var model = new ApiKeyCreateModel
            {
                Name = action.Name,
                ExpiresAt = action.ExpiresAt
            };

            var apiKey = await apiKeyService.CreateApiKeyAsync(model);
            dispatcher.Dispatch(new ApiKeyActions.CreateApiKeySuccessAction(apiKey));
        }
        catch (Exception ex)
        {
            dispatcher.Dispatch(new ApiKeyActions.CreateApiKeyFailureAction(ex.Message));
        }
    }

    [EffectMethod]
    public async Task HandleRevokeApiKeyAction(ApiKeyActions.RevokeApiKeyAction action, IDispatcher dispatcher)
    {
        try
        {
            await apiKeyService.RevokeApiKeyAsync(action.Id);
            dispatcher.Dispatch(new ApiKeyActions.RevokeApiKeySuccessAction(action.Id));
        }
        catch (Exception ex)
        {
            dispatcher.Dispatch(new ApiKeyActions.RevokeApiKeyFailureAction(ex.Message));
        }
    }
}