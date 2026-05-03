using System.Security.Claims;
using FitServiceCRM.Data;
using FitServiceCRM.Models;
using FitServiceCRM.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FitServiceCRM.Controllers;

[Authorize(Roles = nameof(UserRole.Master))]
public sealed class MasterController(AppDbContext db) : Controller
{
    public async Task<IActionResult> Index()
    {
        var currentEmployeeId = await GetCurrentEmployeeIdAsync();
        var model = new MasterDashboardViewModel
        {
            CurrentEmployeeId = currentEmployeeId,
            Orders = await db.WorkOrders
                .Include(order => order.Customer)
                .Include(order => order.Vehicle)
                .Include(order => order.ReceiverEmployee)
                .Include(order => order.OrderServices).ThenInclude(orderService => orderService.ServiceItem)
                .Include(order => order.Executors).ThenInclude(executor => executor.Employee)
                .Include(order => order.WorkItems).ThenInclude(work => work.ServiceItem)
                .Include(order => order.DiagnosticDocuments)
                .Include(order => order.SupportDocuments)
                .OrderByDescending(order => order.PlannedStartAt)
                .Take(40)
                .ToListAsync(),
            Customers = await db.Customers.Include(customer => customer.Vehicles).OrderBy(customer => customer.Name).ToListAsync(),
            Vehicles = await db.Vehicles.Include(vehicle => vehicle.Customer).OrderBy(vehicle => vehicle.PlateNumber).ToListAsync(),
            Services = await db.Services.OrderBy(service => service.Category).ThenBy(service => service.Name).ToListAsync(),
            Employees = await db.Employees.OrderBy(employee => employee.FullName).ToListAsync()
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateOrder(int customerId, int vehicleId, int serviceId, DateTime plannedStartAt, string customerComment)
    {
        var service = await db.Services.FindAsync(serviceId);
        var vehicle = await db.Vehicles.FindAsync(vehicleId);
        var receiverId = await GetCurrentEmployeeIdAsync();
        var executor = await db.Employees.FirstOrDefaultAsync(employee => employee.Position.Contains("Автомеханик"));

        if (service is null || vehicle is null)
        {
            TempData["Message"] = "Не удалось создать заказ-наряд: проверьте услугу и автомобиль.";
            return RedirectToAction(nameof(Index));
        }

        var order = new WorkOrder
        {
            CustomerId = customerId,
            VehicleId = vehicleId,
            ReceiverEmployeeId = receiverId,
            Status = WorkOrderStatus.New,
            CreatedAt = DateTime.Now,
            PlannedStartAt = plannedStartAt,
            PlannedDurationMinutes = service.EstimatedDurationMinutes,
            TotalCost = service.Price,
            CustomerComment = customerComment,
            OrderServices = [new OrderService { ServiceItemId = serviceId }]
        };

        if (executor is not null)
        {
            order.Executors.Add(new WorkOrderEmployee { EmployeeId = executor.Id });
        }

        db.WorkOrders.Add(order);
        await db.SaveChangesAsync();

        db.Notifications.Add(new Notification
        {
            CustomerId = customerId,
            WorkOrderId = order.Id,
            Title = "Запись создана",
            Message = $"Заказ-наряд #{order.Id} создан на {plannedStartAt:dd.MM.yyyy HH:mm}."
        });
        await db.SaveChangesAsync();

        TempData["Message"] = $"Заказ-наряд #{order.Id} создан.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangeStatus(int orderId, WorkOrderStatus status)
    {
        var order = await db.WorkOrders
            .Include(candidate => candidate.Customer)
            .FirstOrDefaultAsync(candidate => candidate.Id == orderId);

        if (order is not null)
        {
            order.Status = status;
            if (status == WorkOrderStatus.Completed)
            {
                order.CompletedAt = DateTime.Now;
                order.ActualDurationMinutes = (int)Math.Max(1, (DateTime.Now - order.PlannedStartAt).TotalMinutes);
            }

            db.Notifications.Add(new Notification
            {
                CustomerId = order.CustomerId,
                WorkOrderId = order.Id,
                Title = status == WorkOrderStatus.Delayed ? "Работы задерживаются" : "Статус заказ-наряда изменен",
                Message = $"Заказ-наряд #{order.Id}: {status.Label()}."
            });

            await db.SaveChangesAsync();
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddWork(int orderId, int serviceId, string description, decimal price)
    {
        var order = await db.WorkOrders
            .Include(candidate => candidate.WorkItems)
            .Include(candidate => candidate.OrderServices)
            .FirstOrDefaultAsync(candidate => candidate.Id == orderId);
        var service = await db.Services.FindAsync(serviceId);

        if (order is not null && service is not null)
        {
            var workPrice = price <= 0 ? service.Price : price;
            order.WorkItems.Add(new WorkItem
            {
                ServiceItemId = service.Id,
                VehicleId = order.VehicleId,
                Description = description,
                Price = workPrice
            });

            if (order.OrderServices.All(orderService => orderService.ServiceItemId != service.Id))
            {
                order.OrderServices.Add(new OrderService { ServiceItemId = service.Id });
            }

            order.TotalCost += workPrice;
            if (order.Status is not WorkOrderStatus.Completed and not WorkOrderStatus.Canceled)
            {
                order.Status = WorkOrderStatus.AwaitingApproval;
            }

            db.Notifications.Add(new Notification
            {
                CustomerId = order.CustomerId,
                WorkOrderId = order.Id,
                Title = "Добавлены работы к согласованию",
                Message = $"{service.Name}: {workPrice:N0} ₽. Требуется подтверждение клиента."
            });

            await db.SaveChangesAsync();
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddDiagnostic(int orderId, string summary)
    {
        var order = await db.WorkOrders.FindAsync(orderId);
        var employeeId = await GetCurrentEmployeeIdAsync();

        if (order is not null)
        {
            db.DiagnosticDocuments.Add(new DiagnosticDocument
            {
                WorkOrderId = order.Id,
                EmployeeId = employeeId,
                CustomerId = order.CustomerId,
                VehicleId = order.VehicleId,
                PerformedAt = DateTime.Now,
                Summary = summary,
                FileName = $"DIAG-{DateTime.Now:yyyyMMdd-HHmm}-WO{order.Id}.pdf"
            });

            order.Status = WorkOrderStatus.AwaitingApproval;
            db.Notifications.Add(new Notification
            {
                CustomerId = order.CustomerId,
                WorkOrderId = order.Id,
                Title = "Диагностика завершена",
                Message = $"По заказ-наряду #{order.Id} сформировано диагностическое заключение."
            });

            await db.SaveChangesAsync();
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateReport(int orderId)
    {
        var order = await db.WorkOrders.FindAsync(orderId);
        if (order is not null)
        {
            db.SupportDocuments.Add(new SupportDocument
            {
                WorkOrderId = order.Id,
                Kind = DocumentKind.WorkReport,
                FileName = $"REPORT-WO{order.Id}-{DateTime.Now:yyyyMMddHHmm}.pdf",
                CreatedAt = DateTime.Now
            });
            await db.SaveChangesAsync();
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteOrder(int orderId)
    {
        var order = await db.WorkOrders.FindAsync(orderId);
        if (order is not null && order.Status != WorkOrderStatus.Completed)
        {
            db.WorkOrders.Remove(order);
            await db.SaveChangesAsync();
            TempData["Message"] = $"Заказ-наряд #{orderId} удален.";
        }

        return RedirectToAction(nameof(Index));
    }

    private async Task<int> GetCurrentEmployeeIdAsync()
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
        var user = await db.Users.AsNoTracking().SingleAsync(candidate => candidate.Id == userId);
        return user.EmployeeId ?? await db.Employees
            .Where(employee => employee.Position.Contains("Мастер"))
            .Select(employee => employee.Id)
            .FirstAsync();
    }
}
