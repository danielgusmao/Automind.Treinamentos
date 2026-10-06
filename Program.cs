using System.Security.Claims;
using Automind.Treinamentos.Data;
using Automind.Treinamentos.Services;
using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.Configure<ActiveDirectoryOptions>(builder.Configuration.GetSection("ActiveDirectory"));
builder.Services.Configure<StorageOptions>(builder.Configuration.GetSection("Storage"));
builder.Services.Configure<TeamsOptions>(builder.Configuration.GetSection("Automind:Teams"));
builder.Services.Configure<PortalOptions>(builder.Configuration.GetSection("Portal"));

builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.Cookie.Name = "Automind.Treinamentos.Session";
    options.Cookie.HttpOnly = true;
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.Cookie.SecurePolicy = CookieSecurePolicy.None;
    options.IdleTimeout = TimeSpan.FromHours(8);
});

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Account/AccessDenied";
        options.Cookie.Name = "Automind.Treinamentos.Auth";
        options.Cookie.HttpOnly = true;
        options.Cookie.SameSite = SameSiteMode.Lax;
        // Ambiente atual sem certificado. Alterar para Always quando HTTPS for habilitado.
        options.Cookie.SecurePolicy = CookieSecurePolicy.None;
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("TreinamentosAdmin", policy => policy.RequireRole("TreinamentosAdmin"));
});

builder.Services.AddSingleton<StorageService>();
builder.Services.AddSingleton<TrainingDb>();
builder.Services.AddSingleton<AuditService>();
builder.Services.AddSingleton<SimplePdfService>();
builder.Services.AddSingleton<TrainingSnapshotService>();
builder.Services.AddHttpClient("TeamsWebhook", client => client.Timeout = TimeSpan.FromSeconds(15));
builder.Services.AddSingleton<TeamsWebhookService>();
builder.Services.AddScoped<TrainingRepository>();
builder.Services.AddScoped<AdAuthenticationService>();
builder.Services.AddScoped<AdAdminAuthorizationService>();
builder.Services.AddScoped<AdDirectoryService>();
builder.Services.AddScoped<EvidenceService>();
builder.Services.AddScoped<DatabaseInitializer>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}

// Nao usar UseHttpsRedirection nesta versao: o ambiente ainda nao possui certificado.
app.UseStaticFiles();
app.UseRouting();
app.UseSession();
app.UseAuthentication();

// A role administrativa e transitoria e existe somente na requisicao atual.
// Roles gravadas em cookies antigos sao removidas antes da consulta LDAP.
app.Use(async (context, next) =>
{
    if (context.User.Identity?.IsAuthenticated == true)
    {
        var identity = context.User.Identities.FirstOrDefault(x => x.IsAuthenticated);
        if (identity is not null)
        {
            foreach (var claim in identity.FindAll(ClaimTypes.Role)
                .Where(c =>
                    string.Equals(c.Value, "TreinamentosAdmin", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(c.Value, "Informatica", StringComparison.OrdinalIgnoreCase))
                .ToList())
            {
                identity.RemoveClaim(claim);
            }

            var sam = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!string.IsNullOrWhiteSpace(sam))
            {
                var adminAuthorization = context.RequestServices.GetRequiredService<AdAdminAuthorizationService>();
                if (adminAuthorization.IsInAdminGroup(sam))
                    identity.AddClaim(new Claim(ClaimTypes.Role, "TreinamentosAdmin"));
            }
        }
    }

    await next();
});

app.UseAuthorization();

using (var scope = app.Services.CreateScope())
{
    var initializer = scope.ServiceProvider.GetRequiredService<DatabaseInitializer>();
    await initializer.InitializeAsync();
}

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Training}/{action=Index}/{id?}");

app.Run();
