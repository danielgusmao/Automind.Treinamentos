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
        options.Events.OnValidatePrincipal = context =>
        {
            var principal = context.Principal;
            var identity = principal?.Identity as ClaimsIdentity;
            var sam = principal?.FindFirstValue(ClaimTypes.NameIdentifier);

            if (identity is null || string.IsNullOrWhiteSpace(sam))
            {
                context.RejectPrincipal();
                return Task.CompletedTask;
            }

            var ad = context.HttpContext.RequestServices.GetRequiredService<AdAuthenticationService>();
            var isAdminNow = ad.IsInAdminGroup(sam);
            var hasAdminRole = principal!.IsInRole("TreinamentosAdmin");
            var legacyClaims = identity.FindAll(ClaimTypes.Role)
                .Where(c => string.Equals(c.Value, "Informatica", StringComparison.OrdinalIgnoreCase))
                .ToList();

            var changed = legacyClaims.Count > 0 || hasAdminRole != isAdminNow;
            foreach (var claim in legacyClaims) identity.RemoveClaim(claim);

            if (hasAdminRole && !isAdminNow)
            {
                foreach (var claim in identity.FindAll(ClaimTypes.Role)
                    .Where(c => string.Equals(c.Value, "TreinamentosAdmin", StringComparison.OrdinalIgnoreCase))
                    .ToList())
                    identity.RemoveClaim(claim);
            }
            else if (!hasAdminRole && isAdminNow)
            {
                identity.AddClaim(new Claim(ClaimTypes.Role, "TreinamentosAdmin"));
            }

            if (changed) context.ShouldRenew = true;
            return Task.CompletedTask;
        };
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
