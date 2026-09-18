using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RenenPortfolio.Models;
using RenenPortfolio.Security;

namespace RenenPortfolio.Controllers;

public class AccountController : Controller
{
    private readonly IUserService _users;
    private readonly ILoginThrottle _throttle;
    private readonly ILogger<AccountController> _logger;

    public AccountController(IUserService users, ILoginThrottle throttle, ILogger<AccountController> logger)
    {
        _users = users;
        _throttle = throttle;
        _logger = logger;
    }

    [AllowAnonymous]
    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToAction("Index", "Projects");
        }

        return View(new LoginViewModel { ReturnUrl = returnUrl });
    }

    [AllowAnonymous]
    [HttpPost]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        var client = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";

        if (_throttle.IsLockedOut(client, out var retryAfter))
        {
            ModelState.AddModelError(string.Empty,
                $"Too many failed attempts. Try again in {Math.Ceiling(retryAfter.TotalMinutes)} minute(s).");
            return View(model);
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var displayName = _users.Validate(model.Username, model.Password);

        if (displayName is null)
        {
            _throttle.RecordFailure(client);
            _logger.LogWarning("Failed sign-in attempt from {Client}.", client);

            ModelState.AddModelError(string.Empty, "That username and password do not match.");
            return View(model);
        }

        _throttle.Reset(client);

        var claims = new List<Claim>
        {
            new(ClaimTypes.Name, displayName),
            new(ClaimTypes.NameIdentifier, model.Username.Trim().ToLowerInvariant())
        };

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(identity),
            new AuthenticationProperties
            {
                IsPersistent = model.RememberMe,
                ExpiresUtc = DateTimeOffset.UtcNow.AddHours(model.RememberMe ? 24 * 14 : 8)
            });

        if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
        {
            return Redirect(model.ReturnUrl);
        }

        return RedirectToAction("Index", "Projects");
    }

    [HttpPost]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction(nameof(Login));
    }
}