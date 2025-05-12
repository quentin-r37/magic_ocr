using Fluxor;
using OcrApp.Client.Redux.Actions;
using OcrApp.Client.Redux.State;

namespace OcrApp.Client.Redux.Reducers;

public static class UserReducers
{
    [ReducerMethod]
    public static UserState OnToggleDarkMode(UserState state, UserActions.ToggleDarkModeAction action)
    {
        return state with { IsDarkMode = !state.IsDarkMode };
    }

    [ReducerMethod]
    public static UserState OnSetDarkMode(UserState state, UserActions.SetDarkModeAction action)
    {
        return state with { IsDarkMode = action.IsDarkMode };
    }

    [ReducerMethod]
    public static UserState OnSetProfilePicture(UserState state, UserActions.SetProfilePicture action)
    {
        return state with { ProfilePicture = action.ProfilePicture };
    }

    [ReducerMethod]
    public static UserState OnSetUserName(UserState state, UserActions.SetUserName action)
    {
        return state with { UserName = action.UserName };
    }

    [ReducerMethod]
    public static UserState OnSetMail(UserState state, UserActions.SetMail action)
    {
        return state with { Email = action.Email };
    }


}