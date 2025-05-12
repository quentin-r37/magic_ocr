using Fluxor;
using OcrApp.Client.Redux.State;

namespace OcrApp.Client.Redux.Features;

public class UserFeature : Feature<UserState>
{
    public override string GetName() => "User";

    protected override UserState GetInitialState()
    {
        return new UserState(false, string.Empty, string.Empty, string.Empty);
    }
}