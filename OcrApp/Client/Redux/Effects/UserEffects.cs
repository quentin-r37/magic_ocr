using System.Net.Http.Headers;
using System.Text.Json;
using Blazored.LocalStorage;
using Fluxor;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using OcrApp.Client.Redux.Actions;
using OcrApp.Client.Redux.State;

namespace OcrApp.Client.Redux.Effects;

public class UserEffects(ILocalStorageService localStorage, IState<UserState> userState, IAccessTokenProvider accessTokenProvider, NavigationManager navigation, IDispatcher dispatcher)
{
    private const string DarkModeKey = "user_dark_mode";

    [EffectMethod]
    public async Task HandleLoadSettings(UserActions.LoadUserSettingsAction action, IDispatcher dispatcher)
    {
        try
        {
            var darkMode = await localStorage.GetItemAsync<bool>(DarkModeKey);
            dispatcher.Dispatch(new UserActions.SetDarkModeAction(darkMode));
            var tokenResult = await accessTokenProvider.RequestAccessToken(new AccessTokenRequestOptions
            {
                Scopes =
                    ["User.Read"]
            });
            if (tokenResult is { Status: AccessTokenResultStatus.RequiresRedirect, InteractiveRequestUrl: not null }) navigation.NavigateTo(tokenResult.InteractiveRequestUrl);
            if (tokenResult.TryGetToken(out var token))
            {
                await LoadUserProfile(dispatcher, token.Value);
            }
        }
        catch (AccessTokenNotAvailableException ex)
        {
            ex.Redirect();
        }
        catch (Exception ex)
        {
            navigation.NavigateTo("/authentication/login");
        }
    }

    [EffectMethod]
    public async Task HandleToggleDarkModeAction(UserActions.ToggleDarkModeAction action, IDispatcher dispatcher)
    {
        await localStorage.SetItemAsync(DarkModeKey, userState.Value.IsDarkMode);
    }

    private async Task LoadUserProfile(IDispatcher dispatcher, string tokenValue)
    {
        var httpClient = new HttpClient();
        var request = new HttpRequestMessage(HttpMethod.Get, "https://graph.microsoft.com/v1.0/me");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", tokenValue);

        var response = await httpClient.SendAsync(request);

        if (response.IsSuccessStatusCode)
        {
            var jsonResponse = await response.Content.ReadAsStringAsync();
            var userProfile = JsonSerializer.Deserialize<UserProfile>(jsonResponse, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (userProfile != null)
            {
                dispatcher.Dispatch(new UserActions.SetUserName(userProfile.DisplayName));
                dispatcher.Dispatch(new UserActions.SetMail(userProfile.Mail));

                await LoadProfilePicture(dispatcher, tokenValue);
            }
        }
    }

    private static async Task LoadProfilePicture(IDispatcher dispatcher, string tokenValue)
    {
        var httpClient = new HttpClient();
        var request = new HttpRequestMessage(HttpMethod.Get, "https://graph.microsoft.com/v1.0/me/photo/$value");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", tokenValue);

        var response = await httpClient.SendAsync(request);

        if (response.IsSuccessStatusCode)
        {
            var byteData = await response.Content.ReadAsByteArrayAsync();
            var imageBase64Data = Convert.ToBase64String(byteData);
            var imageDataUrl = $"data:image/jpg;base64,{imageBase64Data}";
            dispatcher.Dispatch(new UserActions.SetProfilePicture(imageDataUrl));
        }
        else
        {
            var imageDataUrl = "/emptyProfilePicture.png";
            dispatcher.Dispatch(new UserActions.SetProfilePicture(imageDataUrl));
        }
    }

    [EffectMethod(typeof(UserActions.SetDarkModeAction))]
    public async Task HandleDarkModeChange(IDispatcher dispatcher)
    {
        await localStorage.SetItemAsync(DarkModeKey, userState.Value.IsDarkMode);
    }
}