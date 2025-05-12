using OcrApp.Shared;

namespace OcrApp.Client.Redux.State;

public record ApiKeyState(
    bool IsLoading,
    List<ApiKeyModel> ApiKeys,
    string Error,
    bool IsCreating,
    bool IsDeleting,
    string? NewKeyValue);