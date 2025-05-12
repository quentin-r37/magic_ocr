using Fluxor;
using Fluxor.Blazor.Web.Components;
using Microsoft.AspNetCore.Components;
using OcrApp.Client.Redux.Actions;

namespace OcrApp.Client
{
    public abstract class FluxorPage : FluxorComponent
    {
        [Inject]
        protected IDispatcher Dispatcher { get; set; } = default!;
        protected override async Task OnInitializedAsync()
        {
            Dispatcher.Dispatch(new UserActions.LoadUserSettingsAction());
            await base.OnInitializedAsync();
        }
    }
}
