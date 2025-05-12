using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Web;
using OcrApp.Server;
using OcrApp.Server.Db;
using OcrApp.Server.Services;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

builder.Services.AddDbContext<ApiKeyDbContext>(options =>
    options.UseSqlite(
        builder.Configuration.GetConnectionString("DefaultConnection")));

builder.ConfigureAuthenticationSchemes();
builder.Services.AddApiKeyAuthentication();
builder.Services.AddSingleton<MistralService>();
builder.Services.AddSingleton<DocumentProcessingService>();
builder.Services.AddSingleton<JsonExtractorService>();
builder.Services.Configure<ScalarOptions>(options =>
{
    options.WithPreferredScheme("https");
});

var app = builder.Build();

app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedProto | ForwardedHeaders.XForwardedFor
});


await InitDb();

app.MapOpenApi();
app.MapScalarApiReference();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
}
else
{
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseBlazorFrameworkFiles();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.AddScanApi();
app.AddApiKeysApi();

app.MapRazorPages();
app.MapControllers();
app.MapFallbackToFile("index.html");

app.Run();
return;

async Task InitDb()
{
    using var scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<ApiKeyDbContext>();
    await dbContext.Database.MigrateAsync();
}

