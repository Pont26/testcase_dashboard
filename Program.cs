using Radzen;
using TestCaseDashboard.Components;
using Microsoft.EntityFrameworkCore;
using TestCaseDashboard;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents().AddInteractiveServerComponents().AddHubOptions(options => options.MaximumReceiveMessageSize = 10 * 1024 * 1024);
builder.Services.AddControllers();
builder.Services.AddRadzenComponents();
builder.Services.AddRadzenCookieThemeService(options =>
{
    options.Name = "TestCaseDashboardTheme";
    options.Duration = TimeSpan.FromDays(365);
});

builder.Services.AddHttpClient();

// Register the service once, with the correct lifetime.
// A scoped lifetime is a good default for a service that will be used within a single user request.
builder.Services.AddScoped<TestCaseDashboard.mydatabaseService>();

// CRITICAL FIX: Use AddDbContextFactory instead of AddDbContext.
// This registers the factory that your service class relies on.
builder.Services.AddDbContextFactory<TestCaseDashboard.Data.mydatabaseContext>(options =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("mydatabaseConnection"));
});

var app = builder.Build();
var forwardingOptions = new ForwardedHeadersOptions()
{
    ForwardedHeaders = Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.XForwardedFor | Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.XForwardedProto
};
forwardingOptions.KnownNetworks.Clear();
forwardingOptions.KnownProxies.Clear();
app.UseForwardedHeaders(forwardingOptions);

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();
app.MapControllers();
app.UseStaticFiles();
app.UseAntiforgery();
app.MapRazorComponents<App>().AddInteractiveServerRenderMode();
app.Run();
