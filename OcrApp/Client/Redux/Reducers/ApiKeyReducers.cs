using Fluxor;
using OcrApp.Client.Redux.Actions;
using OcrApp.Client.Redux.State;

namespace OcrApp.Client.Redux.Reducers;

public static class ApiKeyReducers
{
    // Load API Keys
    [ReducerMethod]
    public static ApiKeyState ReduceLoadApiKeysAction(ApiKeyState state, ApiKeyActions.LoadApiKeysAction _) =>
        state with { IsLoading = true, Error = string.Empty };

    [ReducerMethod]
    public static ApiKeyState ReduceLoadApiKeysSuccessAction(ApiKeyState state, ApiKeyActions.LoadApiKeysSuccessAction action) =>
        state with { IsLoading = false, ApiKeys = action.ApiKeys };

    [ReducerMethod]
    public static ApiKeyState ReduceLoadApiKeysFailureAction(ApiKeyState state, ApiKeyActions.LoadApiKeysFailureAction action) =>
        state with { IsLoading = false, Error = action.ErrorMessage };

    // Create API Key
    [ReducerMethod]
    public static ApiKeyState ReduceCreateApiKeyAction(ApiKeyState state, ApiKeyActions.CreateApiKeyAction _) =>
        state with { IsCreating = true, Error = string.Empty };

    [ReducerMethod]
    public static ApiKeyState ReduceCreateApiKeySuccessAction(ApiKeyState state, ApiKeyActions.CreateApiKeySuccessAction action)
    {
        var newApiKeys = new List<OcrApp.Shared.ApiKeyModel>(state.ApiKeys) { action.ApiKey };
        return state with { IsCreating = false, ApiKeys = newApiKeys, NewKeyValue = action.ApiKey.Key };
    }

    [ReducerMethod]
    public static ApiKeyState ReduceCreateApiKeyFailureAction(ApiKeyState state, ApiKeyActions.CreateApiKeyFailureAction action) =>
        state with { IsCreating = false, Error = action.ErrorMessage };

    [ReducerMethod]
    public static ApiKeyState ReduceClearNewKeyValueAction(ApiKeyState state, ApiKeyActions.ClearNewKeyValueAction _) =>
        state with { NewKeyValue = string.Empty };

    // Revoke API Key
    [ReducerMethod]
    public static ApiKeyState ReduceRevokeApiKeyAction(ApiKeyState state, ApiKeyActions.RevokeApiKeyAction _) =>
        state with { IsDeleting = true, Error = string.Empty };

    [ReducerMethod]
    public static ApiKeyState ReduceRevokeApiKeySuccessAction(ApiKeyState state, ApiKeyActions.RevokeApiKeySuccessAction action)
    {
        var updatedApiKeys = state.ApiKeys.FindAll(k => k.Id != action.Id);
        return state with { IsDeleting = false, ApiKeys = updatedApiKeys };
    }

    [ReducerMethod]
    public static ApiKeyState ReduceRevokeApiKeyFailureAction(ApiKeyState state, ApiKeyActions.RevokeApiKeyFailureAction action) =>
        state with { IsDeleting = false, Error = action.ErrorMessage };
}