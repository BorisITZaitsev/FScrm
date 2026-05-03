using System.Security.Claims;
using FitServiceCRM.Data;
using FitServiceCRM.Models;
using FitServiceCRM.Services;
using FitServiceCRM.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FitServiceCRM.Controllers;

public sealed class AuthController(AppDbContext db) : Controller
{
    [AllowAnonymous]
    [HttpGet("/login")]
    public IActionResult Login(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectByRole(User.FindFirstValue(ClaimTypes.Role));
        }

        return View(new LoginViewModel { ReturnUrl = returnUrl });
    }

    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    [HttpPost("/login")]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        var user = await db.Users
            .Include(candidate => candidate.Employee)
            .Include(candidate => candidate.Customer)
            .SingleOrDefaultAsync(candidate => candidate.Login == model.Login);

        if (user is null || !user.IsActive || !PasswordService.Verify(model.Password, user.PasswordHash))
        {
            model.Error = "Неверный логин или пароль.";
            return View(model);
        }

        var displayName = user.Employee?.FullName ?? user.Customer?.Name ?? user.Login;
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.Login),
            new(ClaimTypes.Role, user.Role.ToString()),
            new("DisplayName", displayName)
        };

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);

        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

        if (!string.IsNullOrWhiteSpace(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
        {
            return Redirect(model.ReturnUrl);
        }

        return RedirectByRole(user.Role.ToString());
    }

    [Authorize]
    [HttpPost("/logout")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction(nameof(Login));
    }

    private IActionResult RedirectByRole(string? role) => role switch
    {
        nameof(UserRole.Admin) => RedirectToAction("Index", "Admin"),
        nameof(UserRole.Master) => RedirectToAction("Index", "Master"),
        nameof(UserRole.Manager) => RedirectToAction("Index", "Manager"),
        nameof(UserRole.Client) => RedirectToAction("Index", "ClientPortal"),
        _ => RedirectToAction("Login")
    };
}
