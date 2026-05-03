using System.Security.Claims;
using FitServiceCRM.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FitServiceCRM.Controllers;

[Authorize]
public sealed class DashboardController : Controller
{
    public IActionResult Index()
    {
        return User.FindFirstValue(ClaimTypes.Role) switch
        {
            nameof(UserRole.Admin) => RedirectToAction("Index", "Admin"),
            nameof(UserRole.Master) => RedirectToAction("Index", "Master"),
            nameof(UserRole.Manager) => RedirectToAction("Index", "Manager"),
            nameof(UserRole.Client) => RedirectToAction("Index", "ClientPortal"),
            _ => RedirectToAction("Login", "Auth")
        };
    }

    [AllowAnonymous]
    public IActionResult Error() => View();
}
