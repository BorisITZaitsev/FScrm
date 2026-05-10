using System.Security.Claims;
using FitServiceCRM.Data;
using FitServiceCRM.Models;
using FitServiceCRM.Services;
using FitServiceCRM.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FitServiceCRM.Controllers;

[Authorize(Roles = nameof(UserRole.Manager))]
public sealed class ManagerController(AppDbContext db) : Controller
{
    public async Task<IActionResult> Index()
    {
        var model = new ManagerDashboardViewModel
        {
            RecentOrders = await OrdersBaseQuery()
                .OrderByDescending(order => order.UpdatedAt)
                .Take(4)
                .ToListAsync(),
            Customers = await CustomersBaseQuery()
                .OrderByDescending(customer => customer.WorkOrders.Max(order => (DateTime?)order.UpdatedAt) ?? DateTime.MinValue)
                .Take(4)
                .ToListAsync(),
            Services = await db.Services
                .Include(service => service.ServiceCategory)
                .OrderByDescending(service => service.WorkItems.Max(item => (DateTime?)item.ActualEndAt) ?? DateTime.MinValue)
                .Take(4)
                .ToListAsync(),
            Employees = await db.Employees
                .OrderBy(employee => employee.FullName)
                .Take(4)
                .ToListAsync()
        };

        return View(model);
    }

    public async Task<IActionResult> Orders(
        string? q,
        string sort = "date_desc",
        WorkOrderStatus? status = null,
        DateTime? from = null,
        DateTime? to = null,
        int? id = null,
        bool history = false,
        int? representativeId = null,
        int? vehicleId = null,
        int? serviceViewId = null,
        int? employeeViewId = null,
        int? diagnosticId = null)
    {
        var query = OrdersBaseQuery();

        if (!string.IsNullOrWhiteSpace(q))
        {
            query = query.Where(order =>
                order.OrderNumber.Contains(q)
                || (order.Customer != null && (
                    order.Customer.Name.Contains(q)
                    || (order.Customer.IndividualProfile != null && order.Customer.IndividualProfile.FullName.Contains(q))
                    || (order.Customer.LegalProfile != null && order.Customer.LegalProfile.OrganizationName.Contains(q))
                    || order.Customer.Representatives.Any(representative => representative.FullName.Contains(q))))
                || (order.ReceiverEmployee != null && order.ReceiverEmployee.FullName.Contains(q))
                || order.VehiclesInOrder.Any(link => link.Vehicle != null && (
                    link.Vehicle.Make.Contains(q)
                    || link.Vehicle.Model.Contains(q)
                    || link.Vehicle.PlateNumber.Contains(q)
                    || link.Vehicle.Vin.Contains(q)))
                || (order.Vehicle != null && (
                    order.Vehicle.Make.Contains(q)
                    || order.Vehicle.Model.Contains(q)
                    || order.Vehicle.PlateNumber.Contains(q)
                    || order.Vehicle.Vin.Contains(q))));
        }

        if (status is not null)
        {
            query = query.Where(order => order.Status == status);
        }

        if (from is not null)
        {
            query = query.Where(order => order.PlannedStartAt >= from.Value.Date);
        }

        if (to is not null)
        {
            query = query.Where(order => order.PlannedStartAt < to.Value.Date.AddDays(1));
        }

        query = sort switch
        {
            "date" => query.OrderBy(order => order.PlannedStartAt),
            "status" => query.OrderBy(order => order.Status).ThenByDescending(order => order.UpdatedAt),
            "status_desc" => query.OrderByDescending(order => order.Status).ThenByDescending(order => order.UpdatedAt),
            _ => query.OrderByDescending(order => order.PlannedStartAt)
        };

        var selectedOrder = id is null ? null : await OrdersBaseQuery().FirstOrDefaultAsync(order => order.Id == id);
        var model = new ManagerOrdersSectionViewModel
        {
            Orders = await query.ToListAsync(),
            SelectedOrder = selectedOrder,
            SelectedRepresentative = selectedOrder is null || representativeId is null
                ? null
                : selectedOrder.Customer?.Representatives.FirstOrDefault(item => item.Id == representativeId),
            SelectedVehicle = selectedOrder is null || vehicleId is null
                ? null
                : selectedOrder.VehiclesInOrder.Select(link => link.Vehicle).FirstOrDefault(vehicle => vehicle != null && vehicle.Id == vehicleId)
                    ?? selectedOrder.Vehicle,
            SelectedService = selectedOrder is null || serviceViewId is null
                ? null
                : selectedOrder.OrderServices.Select(item => item.ServiceItem).FirstOrDefault(service => service != null && service.Id == serviceViewId)
                    ?? selectedOrder.WorkItems.Select(item => item.ServiceItem).FirstOrDefault(service => service != null && service.Id == serviceViewId),
            SelectedEmployee = selectedOrder is null || employeeViewId is null
                ? null
                : selectedOrder.Executors.Select(item => item.Employee).FirstOrDefault(employee => employee != null && employee.Id == employeeViewId)
                    ?? (selectedOrder.ReceiverEmployee?.Id == employeeViewId ? selectedOrder.ReceiverEmployee : selectedOrder.ServiceManagerEmployee?.Id == employeeViewId ? selectedOrder.ServiceManagerEmployee : null),
            SelectedDiagnostic = selectedOrder is null || diagnosticId is null
                ? null
                : selectedOrder.DiagnosticConclusions.FirstOrDefault(item => item.Id == diagnosticId),
            History = selectedOrder is null ? [] : WorkOrderSupport.BuildHistory(selectedOrder),
            Query = q,
            Sort = sort,
            Status = status,
            From = from,
            To = to,
            ShowHistory = history
        };

        return View(model);
    }

    public async Task<IActionResult> Customers(string? q, string sort = "name", int? id = null, int? representativeId = null, int? vehicleId = null)
    {
        var query = CustomersBaseQuery();
        if (!string.IsNullOrWhiteSpace(q))
        {
            query = query.Where(customer =>
                customer.Name.Contains(q)
                || (customer.IndividualProfile != null && customer.IndividualProfile.FullName.Contains(q))
                || (customer.LegalProfile != null && customer.LegalProfile.OrganizationName.Contains(q))
                || customer.Representatives.Any(representative => representative.FullName.Contains(q))
                || customer.Vehicles.Any(vehicle => vehicle.Make.Contains(q) || vehicle.Model.Contains(q) || vehicle.PlateNumber.Contains(q)));
        }

        query = sort switch
        {
            "type" => query.OrderBy(customer => customer.Type).ThenBy(customer => customer.Name),
            "status" => query.OrderBy(customer => customer.Status).ThenBy(customer => customer.Name),
            _ => query.OrderBy(customer => customer.Name)
        };

        var selected = id is null ? null : await CustomersBaseQuery().FirstOrDefaultAsync(customer => customer.Id == id);
        return View(new ManagerCustomersSectionViewModel
        {
            Customers = await query.ToListAsync(),
            SelectedCustomer = selected,
            SelectedRepresentative = selected?.Representatives.FirstOrDefault(item => item.Id == representativeId),
            SelectedVehicle = selected?.Vehicles.FirstOrDefault(item => item.Id == vehicleId),
            Query = q,
            Sort = sort
        });
    }

    public async Task<IActionResult> Employees(string? q, string sort = "name", int? id = null)
    {
        var query = db.Employees.AsNoTracking().AsQueryable();
        if (!string.IsNullOrWhiteSpace(q))
        {
            query = query.Where(employee =>
                employee.FullName.Contains(q)
                || employee.Position.Contains(q)
                || employee.Phone.Contains(q)
                || employee.Email.Contains(q));
        }

        query = sort switch
        {
            "position" => query.OrderBy(employee => employee.Position).ThenBy(employee => employee.FullName),
            "position_desc" => query.OrderByDescending(employee => employee.Position).ThenBy(employee => employee.FullName),
            _ => query.OrderBy(employee => employee.FullName)
        };

        return View(new ManagerEmployeesSectionViewModel
        {
            Employees = await query.ToListAsync(),
            SelectedEmployee = id is null ? null : await db.Employees.AsNoTracking().FirstOrDefaultAsync(employee => employee.Id == id),
            Query = q,
            Sort = sort
        });
    }

    public async Task<IActionResult> Services(string? q, string sort = "name", string? category = null, int? id = null)
    {
        var query = db.Services.Include(service => service.ServiceCategory).AsNoTracking().AsQueryable();
        if (!string.IsNullOrWhiteSpace(q))
        {
            query = query.Where(service => service.Name.Contains(q) || service.Description.Contains(q));
        }

        if (!string.IsNullOrWhiteSpace(category))
        {
            query = query.Where(service => service.Category == category || (service.ServiceCategory != null && service.ServiceCategory.Name == category));
        }

        query = sort switch
        {
            "price" => query.OrderBy(service => service.BasePrice).ThenBy(service => service.Name),
            "price_desc" => query.OrderByDescending(service => service.BasePrice).ThenBy(service => service.Name),
            _ => query.OrderBy(service => service.Name)
        };

        return View(new ManagerServicesSectionViewModel
        {
            Services = await query.ToListAsync(),
            SelectedService = id is null ? null : await db.Services.Include(service => service.ServiceCategory).AsNoTracking().FirstOrDefaultAsync(service => service.Id == id),
            Query = q,
            Sort = sort,
            Category = category,
            Categories = await db.Services.Select(service => service.Category).Where(value => value != string.Empty).Distinct().OrderBy(value => value).ToListAsync()
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ApproveOrder(int orderId)
    {
        var order = await OrdersBaseQuery().FirstOrDefaultAsync(candidate => candidate.Id == orderId)
            ?? throw new InvalidOperationException("Заказ-наряд не найден.");

        var userId = GetCurrentUserId();
        var approval = order.Approvals.FirstOrDefault(item => item.ApproverRole == ApprovalRole.ServiceManager && item.ApproverUserId == userId);
        if (approval is null)
        {
            approval = new RoleApproval
            {
                WorkOrderId = orderId,
                ApproverUserId = userId,
                ApproverRole = ApprovalRole.ServiceManager
            };
            db.RoleApprovals.Add(approval);
            order.Approvals.Add(approval);
        }

        approval.Decision = ApprovalDecision.Approved;
        approval.DecisionComment = "Согласовано руководителем сервиса.";
        approval.DecidedAt = DateTime.Now;

        var previousStatus = order.Status;
        var clientApproved = order.Approvals.Any(item => item.ApproverRole == ApprovalRole.Client && item.Decision == ApprovalDecision.Approved);
        order.Status = clientApproved ? WorkOrderStatus.Approved : WorkOrderStatus.AwaitingClientApproval;
        order.UpdatedAt = DateTime.Now;
        order.StatusHistory.Add(new WorkOrderStatusHistory
        {
            PreviousStatus = previousStatus,
            NewStatus = order.Status,
            ChangedByUserId = userId,
            Comment = "Руководитель сервиса согласовал заказ-наряд.",
            ChangedAt = DateTime.Now
        });

        var clientUserId = await db.Users.Where(user => user.CustomerId == order.CustomerId && user.Role == UserRole.Client).Select(user => (int?)user.Id).FirstOrDefaultAsync();
        db.Notifications.Add(WorkOrderSupport.CreateCustomerNotification(
            order.Customer!,
            clientApproved ? "Заказ-наряд согласован" : "Ожидается ваше согласование",
            clientApproved
                ? $"Заказ-наряд {order.OrderNumber} согласован руководителем и клиентом."
                : $"Руководитель сервиса согласовал заказ-наряд {order.OrderNumber}. Требуется ваше согласование.",
            NotificationKind.WorkOrderApproval,
            order,
            appUserId: clientUserId));

        await db.SaveChangesAsync();
        TempData["Message"] = clientApproved ? "Заказ-наряд полностью согласован." : "Согласование руководителя сохранено.";
        return RedirectToAction(nameof(Orders), new { id = orderId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RequestChanges(int orderId, string requiredChanges)
    {
        var order = await OrdersBaseQuery().FirstOrDefaultAsync(candidate => candidate.Id == orderId)
            ?? throw new InvalidOperationException("Заказ-наряд не найден.");

        var authorUserId = GetCurrentUserId();
        var recipientUserId = await db.Users
            .Where(user => user.EmployeeId == order.ReceiverEmployeeId)
            .Select(user => (int?)user.Id)
            .FirstOrDefaultAsync() ?? authorUserId;

        db.ChangeRequests.Add(new ChangeRequest
        {
            WorkOrderId = orderId,
            AuthorUserId = authorUserId,
            RecipientUserId = recipientUserId,
            RequiredChanges = requiredChanges.Trim(),
            Status = ChangeRequestStatus.Open,
            CreatedAt = DateTime.Now
        });

        var previousStatus = order.Status;
        order.Status = WorkOrderStatus.NeedsChanges;
        order.UpdatedAt = DateTime.Now;
        order.StatusHistory.Add(new WorkOrderStatusHistory
        {
            PreviousStatus = previousStatus,
            NewStatus = order.Status,
            ChangedByUserId = authorUserId,
            Comment = "Руководитель сервиса запросил изменения по заказ-наряду.",
            ChangedAt = DateTime.Now
        });

        await db.SaveChangesAsync();
        TempData["Message"] = "Запрос на изменение создан.";
        return RedirectToAction(nameof(Orders), new { id = orderId });
    }

    private int GetCurrentUserId() => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");

    private IQueryable<Customer> CustomersBaseQuery() => db.Customers
        .Include(customer => customer.IndividualProfile)
        .Include(customer => customer.LegalProfile)
        .Include(customer => customer.Representatives.OrderBy(item => item.FullName))
        .Include(customer => customer.Vehicles.OrderBy(item => item.PlateNumber))
        .Include(customer => customer.WorkOrders);

    private IQueryable<WorkOrder> OrdersBaseQuery() => db.WorkOrders
        .Include(order => order.Customer).ThenInclude(customer => customer!.IndividualProfile)
        .Include(order => order.Customer).ThenInclude(customer => customer!.LegalProfile)
        .Include(order => order.Customer).ThenInclude(customer => customer!.Representatives)
        .Include(order => order.Vehicle)
        .Include(order => order.VehiclesInOrder).ThenInclude(link => link.Vehicle)
        .Include(order => order.ServiceCenter)
        .Include(order => order.ReceiverEmployee)
        .Include(order => order.ServiceManagerEmployee)
        .Include(order => order.OrderServices).ThenInclude(item => item.ServiceItem)
        .Include(order => order.Executors).ThenInclude(item => item.Employee)
        .Include(order => order.WorkItems).ThenInclude(item => item.ServiceItem)
        .Include(order => order.WorkItems).ThenInclude(item => item.Vehicle)
        .Include(order => order.WorkItems).ThenInclude(item => item.ExecutorEmployee)
        .Include(order => order.DiagnosticConclusions).ThenInclude(item => item.Vehicle)
        .Include(order => order.DiagnosticConclusions).ThenInclude(item => item.ReceiverEmployee)
        .Include(order => order.Approvals).ThenInclude(item => item.ApproverUser)
        .Include(order => order.ChangeRequests).ThenInclude(item => item.AuthorUser)
        .Include(order => order.ChangeRequests).ThenInclude(item => item.RecipientUser)
        .Include(order => order.StatusHistory).ThenInclude(item => item.ChangedByUser)
        .Include(order => order.Attachments).ThenInclude(item => item.UploadedByUser);
}
