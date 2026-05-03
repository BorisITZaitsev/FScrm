using System.Security.Claims;
using FitServiceCRM.Data;
using FitServiceCRM.Models;
using FitServiceCRM.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FitServiceCRM.Controllers;

[Authorize(Roles = nameof(UserRole.Client))]
public sealed class ClientPortalController(AppDbContext db) : Controller
{
    public async Task<IActionResult> Index()
    {
        var customerId = await GetCurrentCustomerIdAsync();
        var model = new ClientDashboardViewModel
        {
            Customer = await db.Customers.SingleAsync(customer => customer.Id == customerId),
            Vehicles = await db.Vehicles.Where(vehicle => vehicle.CustomerId == customerId).OrderBy(vehicle => vehicle.PlateNumber).ToListAsync(),
            Services = await db.Services.OrderBy(service => service.Category).ThenBy(service => service.Name).ToListAsync(),
            Orders = await db.WorkOrders
                .Include(order => order.Vehicle)
                .Include(order => order.OrderServices).ThenInclude(orderService => orderService.ServiceItem)
                .Include(order => order.WorkItems).ThenInclude(work => work.ServiceItem)
                .Include(order => order.DiagnosticDocuments)
                .Include(order => order.SupportDocuments)
                .Where(order => order.CustomerId == customerId)
                .OrderByDescending(order => order.PlannedStartAt)
                .ToListAsync(),
            Notifications = await db.Notifications
                .Where(notification => notification.CustomerId == customerId)
                .OrderByDescending(notification => notification.CreatedAt)
                .Take(12)
                .ToListAsync()
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateProfile(string name, string phone, string email, string address)
    {
        var customerId = await GetCurrentCustomerIdAsync();
        var customer = await db.Customers.FindAsync(customerId);

        if (customer is not null)
        {
            customer.Name = name;
            customer.Phone = phone;
            customer.Email = email;
            customer.Address = address;
            await db.SaveChangesAsync();
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddVehicle(string make, string model, string plateNumber, int year, int mileage)
    {
        db.Vehicles.Add(new Vehicle
        {
            CustomerId = await GetCurrentCustomerIdAsync(),
            Make = make,
            Model = model,
            PlateNumber = plateNumber,
            Year = year,
            Mileage = mileage
        });

        await db.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Book(int vehicleId, int serviceId, DateTime plannedStartAt, string comment)
    {
        var customerId = await GetCurrentCustomerIdAsync();
        var service = await db.Services.FindAsync(serviceId);
        var vehicle = await db.Vehicles.SingleOrDefaultAsync(candidate => candidate.Id == vehicleId && candidate.CustomerId == customerId);
        var master = await db.Employees.FirstAsync(employee => employee.Position.Contains("Мастер"));

        if (service is not null && vehicle is not null)
        {
            var order = new WorkOrder
            {
                CustomerId = customerId,
                VehicleId = vehicle.Id,
                ReceiverEmployeeId = master.Id,
                Status = WorkOrderStatus.New,
                CreatedAt = DateTime.Now,
                PlannedStartAt = plannedStartAt,
                PlannedDurationMinutes = service.EstimatedDurationMinutes,
                TotalCost = service.Price,
                CustomerComment = comment,
                OrderServices = [new OrderService { ServiceItemId = service.Id }]
            };

            db.WorkOrders.Add(order);
            await db.SaveChangesAsync();

            db.Notifications.Add(new Notification
            {
                CustomerId = customerId,
                WorkOrderId = order.Id,
                Title = "Заявка принята",
                Message = $"Запись #{order.Id} создана. Мастер-приемщик подтвердит детали."
            });

            await db.SaveChangesAsync();
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ApproveOrder(int orderId, bool approved)
    {
        var customerId = await GetCurrentCustomerIdAsync();
        var order = await db.WorkOrders.SingleOrDefaultAsync(candidate => candidate.Id == orderId && candidate.CustomerId == customerId);

        if (order is not null)
        {
            order.Status = approved ? WorkOrderStatus.Approved : WorkOrderStatus.Canceled;
            order.ClientApprovedAt = approved ? DateTime.Now : null;

            db.Notifications.Add(new Notification
            {
                CustomerId = customerId,
                WorkOrderId = order.Id,
                Title = approved ? "Работы согласованы" : "Работы отклонены",
                Message = approved
                    ? $"Вы согласовали список работ по заказ-наряду #{order.Id}."
                    : $"Вы отклонили список работ по заказ-наряду #{order.Id}."
            });

            await db.SaveChangesAsync();
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MarkNotificationRead(int notificationId)
    {
        var customerId = await GetCurrentCustomerIdAsync();
        var notification = await db.Notifications.SingleOrDefaultAsync(candidate => candidate.Id == notificationId && candidate.CustomerId == customerId);

        if (notification is not null)
        {
            notification.IsRead = true;
            await db.SaveChangesAsync();
        }

        return RedirectToAction(nameof(Index));
    }

    private async Task<int> GetCurrentCustomerIdAsync()
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
        var customerId = await db.Users
            .Where(user => user.Id == userId)
            .Select(user => user.CustomerId)
            .SingleAsync();

        return customerId ?? throw new InvalidOperationException("Пользователь не связан с клиентом.");
    }
}
