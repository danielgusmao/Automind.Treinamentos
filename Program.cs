using System.Security.Claims;
using System.Threading.RateLimiting;
using Automind.Treinamentos.Data;
using Automind.Treinamentos.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Caching.Memory;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.Configure<ActiveDirectoryOptions>(builder.Configuration.GetSection("ActiveDirectory"));
builder.Services.Configure<StorageOptions>(builder.Configuration.GetSection("Storage"));
builder.Services.Configure<TeamsOptions>(builder.Configuration.GetSection("Automind:Teams"));
builder.Services.Configure<PortalOptions>(builder.Configuration.GetSection("Portal"));

builder.Services.AddDistributedMemoryCache();
builder.Services.AddMemoryCache();
builder.Services.AddHttpContextAccessor();
builder.Services.AddHealthChecks();
builder.Services.AddSession(options =>
{
    options.Cookie.Name = "Automind.Treinamentos.Session";
    options.Cookie.HttpOnly = true;
    options.Cookie.SameSite = SameSiteMode.Lax;
    // Ambiente atual ainda sem certificado. Trocar para Always junto com a ativacao do HTTPS.
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
        // Pendente exclusivamente ate existir certificado TLS no IIS.
        options.Cookie.SecurePolicy = CookieSecurePolicy.None;
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("TreinamentosAdmin", policy => policy.RequireRole("TreinamentosAdmin"));
});

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.AddPolicy("login", httpContext =>
    {
        var ip = httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        return RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: ip,
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 10,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0,
                AutoReplenishment = true
            });
    });
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

// Correlacao simples para diagnostico de requisicoes e auditoria.
app.Use(async (context, next) =>
{
    context.Response.Headers["X-Correlation-ID"] = context.TraceIdentifier;
    await next();
});

// Hardening compativel com o ambiente HTTP atual. HSTS/HTTPS ficam pendentes ate existir certificado.
app.Use(async (context, next) =>
{
    context.Response.Headers["X-Content-Type-Options"] = "nosniff";
    context.Response.Headers["X-Frame-Options"] = "DENY";
    context.Response.Headers["Referrer-Policy"] = "same-origin";
    context.Response.Headers["Permissions-Policy"] = "camera=(), microphone=(), geolocation=()";
    context.Response.Headers["Content-Security-Policy"] = "default-src 'self'; img-src 'self' data:; style-src 'self' 'unsafe-inline'; script-src 'self' 'unsafe-inline'; object-src 'none'; base-uri 'self'; frame-ancestors 'none'; form-action 'self'";
    await next();
});

app.UseStaticFiles();
app.UseRouting();
app.UseRateLimiter();
app.UseSession();
app.UseAuthentication();

// Revalida periodicamente se a conta continua habilitada/elegivel no AD.
// O cache curto evita uma consulta LDAP em toda requisicao, mas reduz a janela de uma sessao revogada.
app.Use(async (context, next) =>
{
    if (context.User.Identity?.IsAuthenticated == true)
    {
        var sam = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!string.IsNullOrWhiteSpace(sam))
        {
            var cache = context.RequestServices.GetRequiredService<IMemoryCache>();
            var cacheKey = $"ad-session-enabled:{sam.ToLowerInvariant()}";
            if (!cache.TryGetValue(cacheKey, out bool enabledAndEligible))
            {
                var ad = context.RequestServices.GetRequiredService<AdAuthenticationService>();
                enabledAndEligible = ad.IsEnabledAndEligible(sam);
                cache.Set(cacheKey, enabledAndEligible, TimeSpan.FromMinutes(2));
            }

            if (!enabledAndEligible)
            {
                await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                context.Session.Clear();
                context.Response.Redirect("/Account/Login");
                return;
            }
        }
    }

    await next();
});

// A role administrativa e transitoria: nunca e confiada a partir de cookie antigo.
// Membership e conta habilitada continuam sendo verificadas no AD em cada requisicao administrativa.
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

app.MapHealthChecks("/health");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Training}/{action=Index}/{id?}");

app.Run();
