using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using RealEstate.Admin;
using RealEstate.Admin.Auth;
using RealEstate.Admin.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

var apiBaseUrl = builder.Configuration.GetValue<string>("ApiBaseUrl") ?? "https://0.0.0.0:7199";

// Token service (localStorage management)
builder.Services.AddScoped<TokenService>();

// Auth state provider
builder.Services.AddScoped<AdminAuthStateProvider>();
builder.Services.AddScoped<AuthenticationStateProvider>(sp => sp.GetRequiredService<AdminAuthStateProvider>());

// Auth handler for attaching Bearer tokens
builder.Services.AddScoped<AuthorizingDelegatingHandler>();

// Public API client (no auth header - for login)
builder.Services.AddHttpClient("PublicApi", client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
});

// Authenticated API client (with Bearer token)
builder.Services.AddHttpClient("AuthenticatedApi", client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
}).AddHttpMessageHandler<AuthorizingDelegatingHandler>();

// Services
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<PermissionService>();
builder.Services.AddScoped<ApiClient>();

// Feature services
builder.Services.AddScoped<DashboardService>();
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<AgentService>();
builder.Services.AddScoped<PropertyService>();
builder.Services.AddScoped<BuilderService>();
builder.Services.AddScoped<ProjectService>();
builder.Services.AddScoped<AuditLogService>();
builder.Services.AddScoped<RoleService>();
builder.Services.AddScoped<ToastService>();

builder.Services.AddAuthorizationCore();

var host = builder.Build();

// Wire up the refresh token callback to avoid circular DI in auth state provider
var authStateProvider = host.Services.GetRequiredService<AdminAuthStateProvider>();
var authService = host.Services.GetRequiredService<AuthService>();
authStateProvider.SetRefreshTokenFunc(() => authService.TryRefreshTokenAsync(notifyAuthState: false));

await host.RunAsync();
