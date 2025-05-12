using Fluxor;
using OcrApp.Client.Redux.State;
using OcrApp.Shared;

namespace OcrApp.Client.Redux.Features;

public class ApiKeyFeature : Feature<ApiKeyState>
{
    public override string GetName() => "ApiKey";

    protected override ApiKeyState GetInitialState()
    {
        return new ApiKeyState(
            IsLoading: false,
            ApiKeys: [],
            Error: string.Empty,
            IsCreating: false,
            IsDeleting: false,
            NewKeyValue: string.Empty);
    }
}