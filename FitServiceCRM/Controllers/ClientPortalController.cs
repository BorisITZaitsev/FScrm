using System.Security.Claims;
using FitServiceCRM.Data;
using FitServiceCRM.Models;
using FitServiceCRM.Services;
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
        var customer = await GetCurrentCustomerAsync();
        var orders = await OrdersBaseQuery(customer.Id).OrderByDescending(order => order.UpdatedAt).Take(4).ToListAsync();
        var diagnostics = await DiagnosticsBaseQuery(customer.Id).OrderByDescending(item => item.CreatedAt).Take(4).ToListAsync();
        var notifications = await NotificationsQuery(customer.Id).OrderByDescending(item => item.CreatedAt).Take(6).ToListAsync();
        var activeOrders = orders.Where(order => order.Status is WorkOrderStatus.AwaitingClientApproval or WorkOrderStatus.Approved or WorkOrderStatus.InProgress or WorkOrderStatus.Delayed).ToList();
        var serviceManager = activeOrders.Select(order => order.ServiceManagerEmployee).FirstOrDefault(employee => employee is not null)
            ?? await db.Employees.FirstOrDefaultAsync(employee => employee.Position.Contains("Руководитель"));

        var contacts = activeOrders
            .SelectMany(order => order.VehiclesInOrder.Select(link => new ClientContactCardViewModel
            {
                Employee = order.ReceiverEmployee!,
                Vehicle = link.Vehicle,
                WorkOrder = order
            }))
            .Where(item => item.Employee is not null)
            .DistinctBy(item => $"{item.Employee.Id}:{item.Vehicle?.Id}:{item.WorkOrder?.Id}")
            .ToList();

        return View(new ClientDashboardViewModel
        {
            Customer = customer,
            Notifications = notifications,
            Vehicles = customer.Vehicles.OrderBy(vehicle => vehicle.PlateNumber).Take(4).ToList(),
            Orders = orders,
            Diagnostics = diagnostics,
            ServiceManager = serviceManager,
            Contacts = contacts
        });
    }

    public async Task<IActionResult> Appointments(string? q, string sort = "date_desc")
    {
        var customer = await GetCurrentCustomerAsync();
        var appointments = await db.Appointments
            .Include(item => item.ServiceCenter)
            .Include(item => item.Services).ThenInclude(link => link.ServiceItem)
            .Include(item => item.Vehicles).ThenInclude(link => link.Vehicle)
            .Where(item => item.CustomerId == customer.Id)
            .OrderByDescending(item => item.PlannedVisitAt)
            .ToListAsync();

        return View(new ClientAppointmentsSectionViewModel
        {
            Customer = customer,
            Vehicles = customer.Vehicles.OrderBy(vehicle => vehicle.PlateNumber).ToList(),
            Services = await db.Services.Include(service => service.ServiceCategory).OrderBy(service => service.Name).ToListAsync(),
            ServiceCenters = await db.ServiceCenters.OrderBy(center => center.Name).ToListAsync(),
            Appointments = appointments,
            Query = q,
            Sort = sort
        });
    }

    public async Task<IActionResult> Profile(int? representativeId = null, bool representativeCreate = false, bool representativeEdit = false)
    {
        var customer = await GetCurrentCustomerAsync();
        return View(new ClientProfileSectionViewModel
        {
            Customer = customer,
            SelectedRepresentative = representativeId is null ? null : customer.Representatives.FirstOrDefault(item => item.Id == representativeId),
            IsRepresentativeCreate = representativeCreate,
            IsRepresentativeEdit = representativeCreate || representativeEdit
        });
    }

    public async Task<IActionResult> Vehicles(string? q, int? id = null, bool create = false, bool edit = false, bool history = false, int? orderId = null, int? diagnosticId = null, int? notificationId = null)
    {
        var customer = await GetCurrentCustomerAsync();
        await MarkNotificationAsReadAsync(customer.Id, notificationId);

        var vehicles = customer.Vehicles.AsEnumerable();
        if (!string.IsNullOrWhiteSpace(q))
        {
            vehicles = vehicles.Where(vehicle =>
                vehicle.Make.Contains(q, StringComparison.OrdinalIgnoreCase)
                || vehicle.Model.Contains(q, StringComparison.OrdinalIgnoreCase)
                || vehicle.PlateNumber.Contains(q, StringComparison.OrdinalIgnoreCase)
                || vehicle.Vin.Contains(q, StringComparison.OrdinalIgnoreCase));
        }

        var selectedVehicle = id is null ? null : customer.Vehicles.FirstOrDefault(vehicle => vehicle.Id == id);
        return View(new ClientVehiclesSectionViewModel
        {
            Customer = customer,
            Vehicles = vehicles.OrderBy(vehicle => vehicle.PlateNumber).ToList(),
            SelectedVehicle = selectedVehicle,
            SelectedOrder = orderId is null ? null : await OrdersBaseQuery(customer.Id).FirstOrDefaultAsync(order => order.Id == orderId),
            SelectedDiagnostic = diagnosticId is null ? null : await DiagnosticsBaseQuery(customer.Id).FirstOrDefaultAsync(item => item.Id == diagnosticId),
            VehicleOrders = selectedVehicle is null
                ? []
                : await OrdersBaseQuery(customer.Id).Where(order => order.VehiclesInOrder.Any(link => link.VehicleId == selectedVehicle.Id)).OrderByDescending(order => order.PlannedStartAt).ToListAsync(),
            VehicleDiagnostics = selectedVehicle is null
                ? []
                : await DiagnosticsBaseQuery(customer.Id).Where(item => item.VehicleId == selectedVehicle.Id).OrderByDescending(item => item.CreatedAt).ToListAsync(),
            Query = q,
            IsCreate = create,
            IsEdit = create || edit,
            ShowHistory = history
        });
    }

    public async Task<IActionResult> Orders(string? q, string sort = "date_desc", WorkOrderStatus? status = null, DateTime? from = null, int? id = null, bool history = false, int? vehicleId = null, int? diagnosticId = null, int? notificationId = null)
    {
        var customer = await GetCurrentCustomerAsync();
        await MarkNotificationAsReadAsync(customer.Id, notificationId);

        var query = OrdersBaseQuery(customer.Id);
        if (!string.IsNullOrWhiteSpace(q))
        {
            query = query.Where(order =>
                order.OrderNumber.Contains(q)
                || order.VehiclesInOrder.Any(link => link.Vehicle != null && (
                    link.Vehicle.Make.Contains(q)
                    || link.Vehicle.Model.Contains(q)
                    || link.Vehicle.PlateNumber.Contains(q))));
        }

        if (status is not null)
        {
            query = query.Where(order => order.Status == status);
        }

        if (from is not null)
        {
            query = query.Where(order => order.PlannedStartAt >= from.Value.Date);
        }

        query = sort switch
        {
            "date" => query.OrderBy(order => order.PlannedStartAt),
            _ => query.OrderByDescending(order => order.PlannedStartAt)
        };

        var selected = id is null ? null : await OrdersBaseQuery(customer.Id).FirstOrDefaultAsync(order => order.Id == id);
        return View(new ClientOrdersSectionViewModel
        {
            Customer = customer,
            Orders = await query.ToListAsync(),
            SelectedOrder = selected,
            SelectedVehicle = selected is null || vehicleId is null ? null : selected.VehiclesInOrder.Select(link => link.Vehicle).FirstOrDefault(vehicle => vehicle != null && vehicle.Id == vehicleId),
            SelectedDiagnostic = selected is null || diagnosticId is null ? null : selected.DiagnosticConclusions.FirstOrDefault(item => item.Id == diagnosticId),
            History = selected is null ? [] : WorkOrderSupport.BuildHistory(selected),
            Query = q,
            Sort = sort,
            Status = status,
            From = from,
            ShowHistory = history
        });
    }

    public async Task<IActionResult> Diagnostics(string? q, string sort = "date_desc", DiagnosticConclusionStatus? status = null, DateTime? from = null, int? id = null, int? notificationId = null)
    {
        var customer = await GetCurrentCustomerAsync();
        await MarkNotificationAsReadAsync(customer.Id, notificationId);

        var query = DiagnosticsBaseQuery(customer.Id).Where(item => item.Status != DiagnosticConclusionStatus.Draft);
        if (!string.IsNullOrWhiteSpace(q))
        {
            query = query.Where(item => item.Vehicle!.Make.Contains(q) || item.Vehicle.Model.Contains(q) || item.Vehicle.PlateNumber.Contains(q));
        }

        if (status is not null)
        {
            query = query.Where(item => item.Status == status);
        }

        if (from is not null)
        {
            query = query.Where(item => item.CreatedAt >= from.Value.Date);
        }

        query = sort switch
        {
            "date" => query.OrderBy(item => item.CreatedAt),
            _ => query.OrderByDescending(item => item.CreatedAt)
        };

        return View(new ClientDiagnosticsSectionViewModel
        {
            Customer = customer,
            Diagnostics = await query.ToListAsync(),
            SelectedDiagnostic = id is null ? null : await DiagnosticsBaseQuery(customer.Id).FirstOrDefaultAsync(item => item.Id == id),
            Query = q,
            Sort = sort,
            Status = status,
            From = from
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveProfile(string phone, string email, string? address)
    {
        var customer = await db.Customers.FindAsync(await GetCurrentCustomerIdAsync()) ?? throw new InvalidOperationException("Клиент не найден.");
        customer.Phone = phone.Trim();
        customer.Email = email.Trim();
        customer.Address = address?.Trim() ?? string.Empty;

        if (customer.IndividualProfile is not null)
        {
            customer.IndividualProfile.Phone = customer.Phone;
            customer.IndividualProfile.Email = customer.Email;
        }

        if (customer.LegalProfile is not null)
        {
            customer.LegalProfile.Phone = customer.Phone;
            customer.LegalProfile.Email = customer.Email;
            customer.LegalProfile.ActualAddress = customer.Address;
        }

        await db.SaveChangesAsync();
        TempData["Message"] = "Профиль сохранен.";
        return RedirectToAction(nameof(Profile));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveRepresentative(int? id, string fullName, string? position, string? phone, string? email)
    {
        var customerId = await GetCurrentCustomerIdAsync();
        var customer = await db.Customers.Include(item => item.Representatives).FirstAsync(item => item.Id == customerId);
        if (customer.Type != CustomerType.Company)
        {
            TempData["Message"] = "Представители доступны только для юридических лиц.";
            return RedirectToAction(nameof(Profile));
        }

        CustomerRepresentative representative;
        if (id is null)
        {
            representative = new CustomerRepresentative { CustomerId = customerId };
            db.CustomerRepresentatives.Add(representative);
        }
        else
        {
            representative = customer.Representatives.FirstOrDefault(item => item.Id == id) ?? throw new InvalidOperationException("Представитель не найден.");
        }

        representative.FullName = fullName.Trim();
        representative.Position = position?.Trim() ?? string.Empty;
        representative.Phone = phone?.Trim() ?? string.Empty;
        representative.Email = email?.Trim() ?? string.Empty;
        await db.SaveChangesAsync();

        TempData["Message"] = id is null ? "Представитель добавлен." : "Представитель сохранен.";
        return RedirectToAction(nameof(Profile), new { representativeId = representative.Id, representativeEdit = true });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveVehicle(int? id, string vin, string make, string model, string plateNumber, int year, int mileage, string? color)
    {
        var customerId = await GetCurrentCustomerIdAsync();
        Vehicle vehicle;
        if (id is null)
        {
            vehicle = new Vehicle { CustomerId = customerId };
            db.Vehicles.Add(vehicle);
        }
        else
        {
            vehicle = await db.Vehicles
                .Include(item => item.OrderLinks).ThenInclude(link => link.WorkOrder)
                .Include(item => item.WorkOrders)
                .FirstOrDefaultAsync(item => item.Id == id && item.CustomerId == customerId)
                ?? throw new InvalidOperationException("Транспортное средство не найдено.");

            if (WorkOrderSupport.HasActiveVehicleOrders(vehicle))
            {
                TempData["Message"] = "Редактирование недоступно, так как по транспортному средству есть активный заказ-наряд.";
                return RedirectToAction(nameof(Vehicles), new { id });
            }
        }

        vehicle.Vin = vin.Trim();
        vehicle.Make = make.Trim();
        vehicle.Model = model.Trim();
        vehicle.PlateNumber = plateNumber.Trim();
        vehicle.Year = year;
        vehicle.Mileage = mileage;
        vehicle.Color = color?.Trim() ?? string.Empty;

        await db.SaveChangesAsync();
        TempData["Message"] = id is null ? "Транспортное средство добавлено." : "Транспортное средство сохранено.";
        return RedirectToAction(nameof(Vehicles), new { id = vehicle.Id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateAppointment(int[] selectedVehicleIds, int[] selectedServiceIds, int serviceCenterId, DateTime plannedVisitAt, string? comment)
    {
        var customer = await GetCurrentCustomerAsync();
        if (customer.Vehicles.Count == 0)
        {
            TempData["Message"] = "Для записи сначала нужно добавить транспортное средство.";
            return RedirectToAction(nameof(Vehicles), new { create = true, edit = true });
        }

        var vehicleIds = selectedVehicleIds.Distinct().ToArray();
        var serviceIds = selectedServiceIds.Distinct().ToArray();
        if (vehicleIds.Length == 0 || serviceIds.Length == 0)
        {
            TempData["Message"] = "Выберите транспортные средства и услуги.";
            return RedirectToAction(nameof(Appointments));
        }

        var appointment = new ServiceAppointment
        {
            CustomerId = customer.Id,
            VehicleId = vehicleIds[0],
            ServiceCenterId = serviceCenterId,
            PlannedVisitAt = plannedVisitAt,
            Status = AppointmentStatus.Created,
            CustomerComment = comment?.Trim() ?? string.Empty,
            CreatedAt = DateTime.Now,
            Services = serviceIds.Select(item => new AppointmentService { ServiceItemId = item }).ToList(),
            Vehicles = vehicleIds.Select(item => new AppointmentVehicle { VehicleId = item }).ToList()
        };

        db.Appointments.Add(appointment);
        await db.SaveChangesAsync();
        TempData["Message"] = "Заявка на запись отправлена.";
        return RedirectToAction(nameof(Appointments));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ApproveOrder(int orderId)
    {
        var customerId = await GetCurrentCustomerIdAsync();
        var order = await OrdersBaseQuery(customerId).FirstOrDefaultAsync(item => item.Id == orderId)
            ?? throw new InvalidOperationException("Заказ-наряд не найден.");

        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
        var approval = order.Approvals.FirstOrDefault(item => item.ApproverRole == ApprovalRole.Client && item.ApproverUserId == userId);
        if (approval is null)
        {
            approval = new RoleApproval
            {
                WorkOrderId = orderId,
                ApproverUserId = userId,
                ApproverRole = ApprovalRole.Client
            };
            db.RoleApprovals.Add(approval);
            order.Approvals.Add(approval);
        }

        var previousStatus = order.Status;
        approval.Decision = ApprovalDecision.Approved;
        approval.DecisionComment = "Согласовано клиентом.";
        approval.DecidedAt = DateTime.Now;
        var managerApproved = order.Approvals.Any(item => item.ApproverRole == ApprovalRole.ServiceManager && item.Decision == ApprovalDecision.Approved);
        order.Status = managerApproved ? WorkOrderStatus.Approved : WorkOrderStatus.AwaitingManagerApproval;
        order.ClientApprovedAt = DateTime.Now;
        order.UpdatedAt = DateTime.Now;
        order.StatusHistory.Add(new WorkOrderStatusHistory
        {
            PreviousStatus = previousStatus,
            NewStatus = order.Status,
            ChangedByUserId = userId,
            Comment = "Клиент согласовал заказ-наряд.",
            ChangedAt = DateTime.Now
        });

        await db.SaveChangesAsync();
        TempData["Message"] = "Согласование сохранено.";
        return RedirectToAction(nameof(Orders), new { id = orderId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RequestChanges(int orderId, string requiredChanges)
    {
        var customerId = await GetCurrentCustomerIdAsync();
        var order = await OrdersBaseQuery(customerId).FirstOrDefaultAsync(item => item.Id == orderId)
            ?? throw new InvalidOperationException("Заказ-наряд не найден.");

        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
        var recipientUserId = await db.Users.Where(user => user.EmployeeId == order.ReceiverEmployeeId).Select(user => (int?)user.Id).FirstOrDefaultAsync() ?? userId;
        db.ChangeRequests.Add(new ChangeRequest
        {
            WorkOrderId = orderId,
            AuthorUserId = userId,
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
            ChangedByUserId = userId,
            Comment = "Клиент запросил изменения по заказ-наряду.",
            ChangedAt = DateTime.Now
        });

        await db.SaveChangesAsync();
        TempData["Message"] = "Запрос на изменение отправлен.";
        return RedirectToAction(nameof(Orders), new { id = orderId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MarkDiagnosticAcknowledged(int diagnosticId)
    {
        var customerId = await GetCurrentCustomerIdAsync();
        var diagnostic = await DiagnosticsBaseQuery(customerId).FirstOrDefaultAsync(item => item.Id == diagnosticId)
            ?? throw new InvalidOperationException("Диагностическое заключение не найдено.");

        diagnostic.Status = DiagnosticConclusionStatus.CustomerAcknowledged;
        await db.SaveChangesAsync();
        TempData["Message"] = "Диагностическое заключение отмечено как просмотренное.";
        return RedirectToAction(nameof(Diagnostics), new { id = diagnosticId });
    }

    private async Task MarkNotificationAsReadAsync(int customerId, int? notificationId)
    {
        if (notificationId is null)
        {
            return;
        }

        var notification = await db.Notifications.FirstOrDefaultAsync(item => item.Id == notificationId && item.CustomerId == customerId);
        if (notification is null || notification.IsRead)
        {
            return;
        }

        notification.IsRead = true;
        notification.ReadAt = DateTime.Now;
        notification.DeliveryStatus = NotificationDeliveryStatus.Read;
        await db.SaveChangesAsync();
    }

    private async Task<Customer> GetCurrentCustomerAsync()
    {
        var customerId = await GetCurrentCustomerIdAsync();
        return await db.Customers
            .Include(customer => customer.IndividualProfile)
            .Include(customer => customer.LegalProfile)
            .Include(customer => customer.Representatives.OrderBy(item => item.FullName))
            .Include(customer => customer.Vehicles.OrderBy(item => item.PlateNumber))
                .ThenInclude(vehicle => vehicle.OrderLinks).ThenInclude(link => link.WorkOrder)
            .Include(customer => customer.Vehicles.OrderBy(item => item.PlateNumber))
                .ThenInclude(vehicle => vehicle.DiagnosticConclusions)
            .FirstAsync(customer => customer.Id == customerId);
    }

    private async Task<int> GetCurrentCustomerIdAsync()
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
        var customerId = await db.Users.Where(user => user.Id == userId).Select(user => user.CustomerId).SingleAsync();
        return customerId ?? throw new InvalidOperationException("Пользователь не связан с клиентом.");
    }

    private IQueryable<WorkOrder> OrdersBaseQuery(int customerId) => db.WorkOrders
        .Where(order => order.CustomerId == customerId)
        .Include(order => order.Customer).ThenInclude(customer => customer!.Representatives)
        .Include(order => order.Vehicle)
        .Include(order => order.VehiclesInOrder).ThenInclude(link => link.Vehicle)
        .Include(order => order.ReceiverEmployee)
        .Include(order => order.ServiceManagerEmployee)
        .Include(order => order.OrderServices).ThenInclude(link => link.ServiceItem)
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

    private IQueryable<DiagnosticConclusion> DiagnosticsBaseQuery(int customerId) => db.DiagnosticConclusions
        .Where(item => item.CustomerId == customerId)
        .Include(item => item.Vehicle)
        .Include(item => item.ReceiverEmployee)
        .Include(item => item.WorkOrder);

    private IQueryable<Notification> NotificationsQuery(int customerId) => db.Notifications
        .Where(item => item.CustomerId == customerId)
        .Include(item => item.WorkOrder)
        .Include(item => item.DiagnosticConclusion)
        .Include(item => item.Vehicle);
}
