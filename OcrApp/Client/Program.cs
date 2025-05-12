using System.Globalization;
using System.Net;
using System.Reflection;
using Blazored.LocalStorage;
using Blazored.Toast;
using Fluxor;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.JSInterop;
using OcrApp.Client;
using OcrApp.Client.Services;
using Refit;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");
builder.Services.AddTransient<CustomAuthorizationMessageHandler>();
builder.Services.AddHttpClient("OcrApp.ServerAPI", client =>
        client.BaseAddress = new Uri($"{builder.HostEnvironment.BaseAddress}"))
    .AddHttpMessageHandler<CustomAuthorizationMessageHandler>();

builder.Services
    .AddRefitClient<IScanApiService>()
    .ConfigureHttpClient(client =>
    {
        client.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress);
        client.Timeout = TimeSpan.FromMinutes(5);
    })
    .AddHttpMessageHandler<CustomAuthorizationMessageHandler>();

builder.Services
    .AddRefitClient<IApiKeyService>()
    .ConfigureHttpClient(client =>
    {
        client.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress);
        client.Timeout = TimeSpan.FromMinutes(5);
    })
    .AddHttpMessageHandler<CustomAuthorizationMessageHandler>();


builder.Services.AddFluxor(options =>
{
    options.ScanAssemblies(Assembly.GetExecutingAssembly()); //.UseReduxDevTools();
});
builder.Services.AddBlazoredLocalStorage();
builder.Services.AddBlazoredToast();

builder.Services.AddMsalAuthentication(options =>
{
    builder.Configuration.Bind("AzureAd", options.ProviderOptions.Authentication);
    options.ProviderOptions.DefaultAccessTokenScopes.Add(
        builder.Configuration.GetSection("ServerApi")["Scopes"]);
    options.ProviderOptions.LoginMode = "redirect";
    options.ProviderOptions.Cache.CacheLocation = "localStorage";
});

builder.Services.AddAuthorizationCore();

var app = builder.Build();
await app.UseDefaultCulture();
await app.RunAsync();

namespace OcrApp.Client
{
    internal class CustomAuthorizationMessageHandler(IAccessTokenProvider provider, NavigationManager navigationManager)
        : BaseAddressAuthorizationMessageHandler(provider, navigationManager)
    {
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            try
            {
                return await base.SendAsync(request, cancellationToken);
            }
            catch (AccessTokenNotAvailableException ex)
            {
                ex.Redirect();
                return new HttpResponseMessage(HttpStatusCode.Unauthorized)
                {
                    Content = new StringContent("Access token is not available or expired.")
                };
            }
        }
    }

    internal static class WebAssemblyHostExtensions
    {
        public static async Task UseDefaultCulture(this WebAssemblyHost host)
        {
            var jsInterop = host.Services.GetRequiredService<IJSRuntime>();
            var storage = host.Services.GetRequiredService<ILocalStorageService>();

            var defaultCulture = new CultureInfo("fr-FR");
            try
            {
                var storedCulture = await storage.GetItemAsStringAsync("BlazorCulture");
                if (string.IsNullOrEmpty(storedCulture))
                    storedCulture = await jsInterop.InvokeAsync<string>("getBrowserLanguage");

                var culture = !string.IsNullOrEmpty(storedCulture)
                    ? new CultureInfo(storedCulture)
                    : defaultCulture;

                CultureInfo.DefaultThreadCurrentCulture = culture;
                CultureInfo.DefaultThreadCurrentUICulture = culture;
            }
            catch (Exception)
            {
                defaultCulture = new CultureInfo("fr-FR");
                CultureInfo.DefaultThreadCurrentCulture = defaultCulture;
                CultureInfo.DefaultThreadCurrentUICulture = defaultCulture;
                throw;
            }
        }
    }
}