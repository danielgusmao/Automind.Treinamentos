using System.Security.Claims;
using Automind.Treinamentos.Models;
using Automind.Treinamentos.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Automind.Treinamentos.Controllers;

public sealed class AccountController : Controller
{
    private readonly AdAuthenticationService _ad;
    private readonly AuditService _audit;
    private readonly AdAdminAuthorizationService _adminAuthorization;

    public AccountController(AdAuthenticationService ad, AuditService audit, AdAdminAuthorizationService adminAuthorization)
    {
        _ad = ad;
        _audit = audit;
        _adminAuthorization = adminAuthorization;
    }

    [AllowAnonymous]
    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true) return RedirectToAction("Index", "Training");
        return View(new LoginViewModel { ReturnUrl = returnUrl });
    }

    [AllowAnonymous]
    [HttpPost]
    [ValidateAntiForgeryToken]
    [EnableRateLimiting("login")]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        AdUser? adUser;
        try
        {
            adUser = _ad.Authenticate(model.Username, model.Password);
        }
        catch
        {
            adUser = null;
        }
        finally
        {
            // A senha nunca e persistida, registrada ou adicionada a claims.
            model.Password = "";
        }

        if (adUser is null)
        {
            await _audit.WriteAsync("login", "failed", model.Username, new { reason = "invalid-credentials-or-ad-error" });
            model.ErrorMessage = "Usuário ou senha inválidos, ou não foi possível validar no Active Directory.";
            return View(model);
        }

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, adUser.SamAccountName),
            new(ClaimTypes.Name, adUser.DisplayName),
            new(ClaimTypes.Email, adUser.Email ?? ""),
            new("job_title", adUser.JobTitle ?? ""),
            new("department", adUser.Department ?? "")
        };
        // Nenhuma role administrativa e persistida no cookie.
        // A autorizacao e reconsultada diretamente no AD em cada requisicao.
        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity));
        var isAdminNow = _adminAuthorization.IsInAdminGroup(adUser.SamAccountName);
        await _audit.WriteAsync("login", "success", adUser.SamAccountName, new { adUser.DisplayName, adUser.Email, admin = isAdminNow });

        if (!string.IsNullOrWhiteSpace(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
            return LocalRedirect(model.ReturnUrl);
        return RedirectToAction("Index", "Training");
    }

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        var actor = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "unknown";
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        HttpContext.Session.Clear();
        await _audit.WriteAsync("logout", "success", actor);
        return RedirectToAction(nameof(Login));
    }

    [AllowAnonymous]
    public IActionResult AccessDenied() => View();
}
