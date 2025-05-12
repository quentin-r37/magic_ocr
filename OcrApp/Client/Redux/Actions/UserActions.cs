namespace OcrApp.Client.Redux.Actions;

public class UserActions
{
    public record SetDarkModeAction(bool IsDarkMode);
    public record ToggleDarkModeAction();
    public record LoadUserSettingsAction();
    public record SetProfilePicture(string ProfilePicture);
    public record SetUserName(string UserName);
    public record SetMail(string Email);

}