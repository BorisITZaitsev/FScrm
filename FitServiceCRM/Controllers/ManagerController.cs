using FitServiceCRM.Data;
using FitServiceCRM.Models;
using FitServiceCRM.Services;
using FitServiceCRM.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FitServiceCRM.Controllers;

[Authorize(Roles = nameof(UserRole.Manager))]
public sealed class ManagerController(AppDbContext db, AnalyticsService analytics) : Controller
{
    public async Task<IActionResult> Index(int? serviceId, int? employeeId, DateTime? from, DateTime? to)
    {
        from ??= DateTime.Today.AddMonths(-12);
        to ??= DateTime.Today.AddDays(1);

        var model = new ManagerDashboardViewModel
        {
            ServiceId = serviceId,
            EmployeeId = employeeId,
            From = from,
            To = to,
            Analytics = await analytics.BuildAsync(serviceId, employeeId, from, to),
            Services = await db.Services.AsNoTracking().OrderBy(service => service.Category).ThenBy(service => service.Name).ToListAsync(),
            Employees = await db.Employees.AsNoTracking().OrderBy(employee => employee.FullName).ToListAsync(),
            Customers = await db.Customers.AsNoTracking().OrderBy(customer => customer.Name).ToListAsync(),
            RecentOrders = await db.WorkOrders
                .AsNoTracking()
                .Include(order => order.Customer)
                .Include(order => order.Vehicle)
                .Include(order => order.ReceiverEmployee)
                .Include(order => order.OrderServices).ThenInclude(orderService => orderService.ServiceItem)
                .OrderByDescending(order => order.PlannedStartAt)
                .Take(18)
                .ToListAsync()
        };

        return View(model);
    }
}
