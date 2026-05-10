using System.Security.Claims;
using FitServiceCRM.Data;
using FitServiceCRM.Models;
using FitServiceCRM.Services;
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
        var model = new MasterDashboardViewModel
        {
            RecentOrders = await OrdersBaseQuery().OrderByDescending(order => order.UpdatedAt).Take(4).ToListAsync(),
            Diagnostics = await DiagnosticsBaseQuery().OrderByDescending(item => item.CreatedAt).Take(4).ToListAsync(),
            Customers = await CustomersBaseQuery().OrderByDescending(customer => customer.WorkOrders.Max(order => (DateTime?)order.UpdatedAt) ?? DateTime.MinValue).Take(4).ToListAsync(),
            Services = await db.Services.Include(service => service.ServiceCategory).OrderBy(service => service.Name).Take(4).ToListAsync()
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
        bool create = false,
        bool edit = false,
        int? workItemId = null,
        bool workItemCreate = false,
        bool workItemEdit = false,
        bool history = false,
        int? sourceDiagnosticId = null)
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
                    || link.Vehicle.Vin.Contains(q))));
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
            _ => query.OrderByDescending(order => order.PlannedStartAt)
        };

        var selectedOrder = id is null ? null : await OrdersBaseQuery().FirstOrDefaultAsync(order => order.Id == id);
        var sourceDiagnostic = sourceDiagnosticId is null ? null : await DiagnosticsBaseQuery().FirstOrDefaultAsync(item => item.Id == sourceDiagnosticId);

        return View(new MasterOrdersSectionViewModel
        {
            CurrentEmployeeId = await GetCurrentEmployeeIdAsync(),
            Orders = await query.ToListAsync(),
            SelectedOrder = selectedOrder,
            SelectedWorkItem = edit && id is not null && workItemId is not null
                ? await db.WorkItems.Include(item => item.Vehicle).Include(item => item.ServiceItem).Include(item => item.ExecutorEmployee).FirstOrDefaultAsync(item => item.Id == workItemId && item.WorkOrderId == id)
                : null,
            SelectedDiagnostic = sourceDiagnostic,
            Customers = await CustomersBaseQuery().OrderBy(customer => customer.Name).ToListAsync(),
            Vehicles = await db.Vehicles.Include(vehicle => vehicle.Customer).OrderBy(vehicle => vehicle.PlateNumber).ToListAsync(),
            Employees = await db.Employees.OrderBy(employee => employee.FullName).ToListAsync(),
            Services = await db.Services.Include(service => service.ServiceCategory).OrderBy(service => service.Name).ToListAsync(),
            ServiceCenters = await db.ServiceCenters.OrderBy(center => center.Name).ToListAsync(),
            Diagnostics = await DiagnosticsBaseQuery().OrderByDescending(item => item.CreatedAt).ToListAsync(),
            History = selectedOrder is null ? [] : WorkOrderSupport.BuildHistory(selectedOrder),
            Query = q,
            Sort = sort,
            Status = status,
            From = from,
            To = to,
            IsCreate = create,
            IsEdit = create || edit,
            IsWorkItemCreate = edit && id is not null && workItemCreate,
            IsWorkItemEdit = edit && id is not null && (workItemCreate || workItemId is not null && workItemEdit),
            ShowHistory = history,
            SourceDiagnosticId = sourceDiagnosticId
        });
    }

    public async Task<IActionResult> Diagnostics(
        string? q,
        string sort = "date_desc",
        DiagnosticConclusionStatus? status = null,
        DateTime? from = null,
        DateTime? to = null,
        int? id = null,
        bool create = false,
        bool edit = false,
        int? orderId = null)
    {
        var query = DiagnosticsBaseQuery();
        if (!string.IsNullOrWhiteSpace(q))
        {
            query = query.Where(item =>
                item.Vehicle!.Make.Contains(q)
                || item.Vehicle.Model.Contains(q)
                || item.Vehicle.PlateNumber.Contains(q)
                || item.Vehicle.Vin.Contains(q)
                || item.Customer!.Name.Contains(q)
                || item.ReceiverEmployee!.FullName.Contains(q));
        }

        if (status is not null)
        {
            query = query.Where(item => item.Status == status);
        }

        if (from is not null)
        {
            query = query.Where(item => item.CreatedAt >= from.Value.Date);
        }

        if (to is not null)
        {
            query = query.Where(item => item.CreatedAt < to.Value.Date.AddDays(1));
        }

        query = sort switch
        {
            "date" => query.OrderBy(item => item.CreatedAt),
            "status" => query.OrderBy(item => item.Status).ThenByDescending(item => item.CreatedAt),
            _ => query.OrderByDescending(item => item.CreatedAt)
        };

        return View(new MasterDiagnosticsSectionViewModel
        {
            CurrentEmployeeId = await GetCurrentEmployeeIdAsync(),
            Diagnostics = await query.ToListAsync(),
            SelectedDiagnostic = id is null ? null : await DiagnosticsBaseQuery().FirstOrDefaultAsync(item => item.Id == id),
            SelectedOrder = orderId is null ? null : await OrdersBaseQuery().FirstOrDefaultAsync(order => order.Id == orderId),
            Customers = await CustomersBaseQuery().OrderBy(customer => customer.Name).ToListAsync(),
            Vehicles = await db.Vehicles.Include(vehicle => vehicle.Customer).OrderBy(vehicle => vehicle.PlateNumber).ToListAsync(),
            Employees = await db.Employees.OrderBy(employee => employee.FullName).ToListAsync(),
            Orders = await OrdersBaseQuery().OrderByDescending(order => order.PlannedStartAt).ToListAsync(),
            Query = q,
            Sort = sort,
            Status = status,
            From = from,
            To = to,
            IsCreate = create,
            IsEdit = create || edit
        });
    }

    public async Task<IActionResult> Customers(string? q, string sort = "name", int? id = null, int? representativeId = null, int? vehicleId = null)
        => View("~/Views/Manager/Customers.cshtml", await BuildCustomerSectionAsync(q, sort, id, representativeId, vehicleId));

    public async Task<IActionResult> Employees(string? q, string sort = "name", int? id = null)
        => View("~/Views/Manager/Employees.cshtml", await BuildEmployeeSectionAsync(q, sort, id));

    public async Task<IActionResult> Services(string? q, string sort = "name", string? category = null, int? id = null)
        => View("~/Views/Manager/Services.cshtml", await BuildServicesSectionAsync(q, sort, category, id));

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveOrder(
        int? id,
        string? orderNumber,
        int customerId,
        int vehicleId,
        int[]? selectedVehicleIds,
        int serviceCenterId,
        int receiverEmployeeId,
        int? serviceManagerEmployeeId,
        WorkOrderStatus status,
        DateTime plannedStartAt,
        DateTime? plannedEndAt,
        DateTime? actualStartAt,
        DateTime? actualEndAt,
        decimal totalCost,
        string? customerComment,
        string? comment,
        int serviceId,
        int? executorEmployeeId,
        int[]? selectedDiagnosticIds)
    {
        WorkOrder order;
        var isCreate = id is null;
        if (isCreate)
        {
            order = new WorkOrder { CreatedAt = DateTime.Now };
            db.WorkOrders.Add(order);
        }
        else
        {
            order = await OrdersBaseQuery().FirstOrDefaultAsync(candidate => candidate.Id == id)
                ?? throw new InvalidOperationException("Заказ-наряд не найден.");
        }

        var service = await db.Services.FindAsync(serviceId) ?? throw new InvalidOperationException("Услуга не найдена.");
        var linkedVehicleIds = (selectedVehicleIds ?? []).Append(vehicleId).Where(candidate => candidate > 0).Distinct().ToArray();
        if (linkedVehicleIds.Length == 0)
        {
            TempData["Message"] = "Выберите хотя бы одно транспортное средство.";
            return RedirectToAction(nameof(Orders), new { id, edit = true, create = isCreate });
        }

        var previousStatus = order.Status;
        var generatedNumber = $"FS-{DateTime.Now:yyyyMMdd}-{(id ?? await db.WorkOrders.CountAsync() + 1):D4}";
        var primaryVehicleId = linkedVehicleIds[0];
        order.OrderNumber = string.IsNullOrWhiteSpace(orderNumber) ? generatedNumber : orderNumber.Trim();
        order.CustomerId = customerId;
        order.VehicleId = primaryVehicleId;
        order.ServiceCenterId = serviceCenterId;
        order.ReceiverEmployeeId = receiverEmployeeId;
        order.ServiceManagerEmployeeId = serviceManagerEmployeeId;
        order.Status = status;
        order.PlannedStartAt = plannedStartAt;
        order.PlannedEndAt = plannedEndAt;
        order.ActualStartAt = actualStartAt;
        order.ActualEndAt = actualEndAt;
        order.CompletedAt = status == WorkOrderStatus.Completed ? actualEndAt ?? DateTime.Now : null;
        order.PlannedDurationMinutes = plannedEndAt is null ? service.StandardDurationMinutes : (int)Math.Max(1, (plannedEndAt.Value - plannedStartAt).TotalMinutes);
        order.ActualDurationMinutes = actualStartAt is not null && actualEndAt is not null ? (int)Math.Max(1, (actualEndAt.Value - actualStartAt.Value).TotalMinutes) : null;
        order.TotalCost = totalCost;
        order.CustomerComment = customerComment?.Trim() ?? string.Empty;
        order.Comment = comment?.Trim() ?? string.Empty;
        order.InternalComment = order.Comment;
        order.UpdatedAt = DateTime.Now;

        order.ServiceAppointment ??= new ServiceAppointment { CreatedAt = DateTime.Now };
        order.ServiceAppointment.CustomerId = customerId;
        order.ServiceAppointment.VehicleId = primaryVehicleId;
        order.ServiceAppointment.ServiceCenterId = serviceCenterId;
        order.ServiceAppointment.ReceiverEmployeeId = receiverEmployeeId;
        order.ServiceAppointment.PlannedVisitAt = plannedStartAt;
        order.ServiceAppointment.Status = status == WorkOrderStatus.Canceled ? AppointmentStatus.Canceled : status is WorkOrderStatus.InProgress or WorkOrderStatus.Delayed ? AppointmentStatus.InWork : AppointmentStatus.Confirmed;
        order.ServiceAppointment.CustomerComment = order.CustomerComment;
        order.ServiceAppointment.Services.Clear();
        order.ServiceAppointment.Services.Add(new AppointmentService { ServiceItemId = serviceId });
        SyncAppointmentVehicles(order.ServiceAppointment, linkedVehicleIds);

        order.OrderServices.Clear();
        order.OrderServices.Add(new OrderService { WorkOrder = order, ServiceItemId = serviceId });

        order.Executors.Clear();
        if (executorEmployeeId is not null)
        {
            order.Executors.Add(new WorkOrderEmployee { WorkOrder = order, EmployeeId = executorEmployeeId.Value });
        }

        SyncOrderVehicles(order, linkedVehicleIds);

        var firstWorkItem = order.WorkItems.OrderBy(item => item.Id).FirstOrDefault();
        if (firstWorkItem is null)
        {
            firstWorkItem = new WorkItem { WorkOrder = order };
            order.WorkItems.Add(firstWorkItem);
        }

        firstWorkItem.VehicleId = primaryVehicleId;
        firstWorkItem.ServiceItemId = serviceId;
        firstWorkItem.ExecutorEmployeeId = executorEmployeeId;
        firstWorkItem.Description = string.IsNullOrWhiteSpace(firstWorkItem.Description) ? $"{service.Name}: основная работа по заказ-наряду" : firstWorkItem.Description;
        firstWorkItem.Quantity = Math.Max(1, firstWorkItem.Quantity);
        firstWorkItem.UnitPrice = totalCost;
        firstWorkItem.Discount = 0;
        firstWorkItem.TotalAmount = totalCost;
        firstWorkItem.Price = totalCost;
        firstWorkItem.Status = status switch
        {
            WorkOrderStatus.Completed => WorkItemStatus.Completed,
            WorkOrderStatus.AwaitingClientApproval or WorkOrderStatus.AwaitingManagerApproval => WorkItemStatus.AwaitingApproval,
            WorkOrderStatus.Canceled => WorkItemStatus.Canceled,
            _ => WorkItemStatus.InProgress
        };
        firstWorkItem.PlannedStartAt = plannedStartAt;
        firstWorkItem.PlannedEndAt = plannedEndAt;
        firstWorkItem.ActualStartAt = actualStartAt;
        firstWorkItem.ActualEndAt = actualEndAt;

        SyncApprovals(order, await GetCurrentUserIdAsync());

        if (selectedDiagnosticIds is not null)
        {
            var diagnostics = await db.DiagnosticConclusions.Where(item => selectedDiagnosticIds.Contains(item.Id)).ToListAsync();
            foreach (var diagnostic in diagnostics)
            {
                diagnostic.WorkOrderId = order.Id;
            }
        }

        if (isCreate || previousStatus != status)
        {
            order.StatusHistory.Add(new WorkOrderStatusHistory
            {
                PreviousStatus = isCreate ? null : previousStatus,
                NewStatus = status,
                ChangedByUserId = await GetCurrentUserIdAsync(),
                Comment = isCreate ? "Заказ-наряд создан мастером-приемщиком." : "Статус заказ-наряда обновлен мастером-приемщиком.",
                ChangedAt = DateTime.Now
            });
        }

        await db.SaveChangesAsync();
        await NotifyCustomerAboutOrderAsync(order);

        TempData["Message"] = isCreate ? "Заказ-наряд создан." : "Заказ-наряд сохранен.";
        return RedirectToAction(nameof(Orders), new { id = order.Id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveWorkItem(
        int? id,
        int orderId,
        int vehicleId,
        int serviceId,
        int? executorEmployeeId,
        string description,
        decimal quantity,
        decimal unitPrice,
        decimal discount,
        WorkItemStatus status,
        DateTime? plannedStartAt,
        DateTime? plannedEndAt,
        DateTime? actualStartAt,
        DateTime? actualEndAt)
    {
        var order = await OrdersBaseQuery().FirstOrDefaultAsync(candidate => candidate.Id == orderId)
            ?? throw new InvalidOperationException("Заказ-наряд не найден.");
        var service = await db.Services.FindAsync(serviceId) ?? throw new InvalidOperationException("Услуга не найдена.");

        WorkItem workItem;
        var isCreate = id is null;
        if (isCreate)
        {
            workItem = new WorkItem { WorkOrderId = orderId };
            db.WorkItems.Add(workItem);
        }
        else
        {
            workItem = await db.WorkItems.FirstOrDefaultAsync(candidate => candidate.Id == id && candidate.WorkOrderId == orderId)
                ?? throw new InvalidOperationException("Работа не найдена.");
        }

        if (order.VehiclesInOrder.All(link => link.VehicleId != vehicleId))
        {
            order.VehiclesInOrder.Add(new WorkOrderVehicle { WorkOrderId = orderId, VehicleId = vehicleId });
        }

        workItem.VehicleId = vehicleId;
        workItem.ServiceItemId = serviceId;
        workItem.ExecutorEmployeeId = executorEmployeeId;
        workItem.Description = description.Trim();
        workItem.Quantity = quantity <= 0 ? 1 : quantity;
        workItem.UnitPrice = unitPrice <= 0 ? service.BasePrice : unitPrice;
        workItem.Discount = discount < 0 ? 0 : discount;
        workItem.TotalAmount = Math.Round(workItem.Quantity * workItem.UnitPrice * (1 - workItem.Discount / 100m), 2, MidpointRounding.AwayFromZero);
        workItem.Price = workItem.TotalAmount;
        workItem.Status = status;
        workItem.PlannedStartAt = plannedStartAt;
        workItem.PlannedEndAt = plannedEndAt;
        workItem.ActualStartAt = actualStartAt;
        workItem.ActualEndAt = actualEndAt;

        if (order.OrderServices.All(link => link.ServiceItemId != serviceId))
        {
            order.OrderServices.Add(new OrderService { WorkOrderId = orderId, ServiceItemId = serviceId });
        }

        order.UpdatedAt = DateTime.Now;
        RecalculateOrderTotal(order);
        order.StatusHistory.Add(new WorkOrderStatusHistory
        {
            PreviousStatus = order.Status,
            NewStatus = order.Status,
            ChangedByUserId = await GetCurrentUserIdAsync(),
            Comment = isCreate ? "В заказ-наряд добавлена работа." : "Работа в заказ-наряде изменена.",
            ChangedAt = DateTime.Now
        });

        await db.SaveChangesAsync();
        TempData["Message"] = isCreate ? "Работа добавлена." : "Работа сохранена.";
        return RedirectToAction(nameof(Orders), new { id = orderId, edit = true, workItemId = workItem.Id, workItemEdit = true });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangeOrderStatus(int orderId, WorkOrderStatus status)
    {
        var order = await OrdersBaseQuery().FirstOrDefaultAsync(candidate => candidate.Id == orderId)
            ?? throw new InvalidOperationException("Заказ-наряд не найден.");

        var previousStatus = order.Status;
        order.Status = status;
        order.UpdatedAt = DateTime.Now;
        if (status == WorkOrderStatus.Completed)
        {
            order.CompletedAt = DateTime.Now;
        }

        order.StatusHistory.Add(new WorkOrderStatusHistory
        {
            PreviousStatus = previousStatus,
            NewStatus = status,
            ChangedByUserId = await GetCurrentUserIdAsync(),
            Comment = "Статус заказ-наряда изменен мастером-приемщиком.",
            ChangedAt = DateTime.Now
        });

        await db.SaveChangesAsync();
        await NotifyCustomerAboutOrderAsync(order);

        TempData["Message"] = "Статус заказ-наряда обновлен.";
        return RedirectToAction(nameof(Orders), new { id = orderId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CancelOrder(int orderId)
    {
        return await ChangeOrderStatus(orderId, WorkOrderStatus.Canceled);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveDiagnostic(
        int? id,
        int customerId,
        int vehicleId,
        int? workOrderId,
        string diagnosticResult,
        string recommendations,
        DiagnosticConclusionStatus status)
    {
        DiagnosticConclusion diagnostic;
        var isCreate = id is null;
        if (isCreate)
        {
            diagnostic = new DiagnosticConclusion { CreatedAt = DateTime.Now, ReceiverEmployeeId = await GetCurrentEmployeeIdAsync() };
            db.DiagnosticConclusions.Add(diagnostic);
        }
        else
        {
            diagnostic = await DiagnosticsBaseQuery().FirstOrDefaultAsync(item => item.Id == id)
                ?? throw new InvalidOperationException("Диагностическое заключение не найдено.");

            if (diagnostic.Status == DiagnosticConclusionStatus.CustomerAcknowledged)
            {
                TempData["Message"] = "Редактирование недоступно: клиент уже ознакомился с заключением.";
                return RedirectToAction(nameof(Diagnostics), new { id });
            }
        }

        diagnostic.CustomerId = customerId;
        diagnostic.VehicleId = vehicleId;
        diagnostic.WorkOrderId = workOrderId;
        diagnostic.DiagnosticResult = diagnosticResult.Trim();
        diagnostic.Recommendations = recommendations.Trim();
        diagnostic.Status = status;

        await db.SaveChangesAsync();

        if (status == DiagnosticConclusionStatus.Formed)
        {
            var customer = await db.Customers.FindAsync(customerId) ?? throw new InvalidOperationException("Клиент не найден.");
            var appUserId = await db.Users.Where(user => user.CustomerId == customerId && user.Role == UserRole.Client).Select(user => (int?)user.Id).FirstOrDefaultAsync();
            db.Notifications.Add(WorkOrderSupport.CreateCustomerNotification(
                customer,
                "Диагностика завершена",
                "Сформировано новое диагностическое заключение.",
                NotificationKind.DiagnosticFinished,
                diagnostic: diagnostic,
                vehicle: await db.Vehicles.FindAsync(vehicleId),
                appUserId: appUserId));
            await db.SaveChangesAsync();
        }

        TempData["Message"] = isCreate ? "Диагностическое заключение создано." : "Диагностическое заключение сохранено.";
        return RedirectToAction(nameof(Diagnostics), new { id = diagnostic.Id });
    }

    public IActionResult CreateOrderFromDiagnostic(int diagnosticId)
        => RedirectToAction(nameof(Orders), new { create = true, edit = true, sourceDiagnosticId = diagnosticId });

    private async Task NotifyCustomerAboutOrderAsync(WorkOrder order)
    {
        var customer = await db.Customers.FindAsync(order.CustomerId);
        if (customer is null)
        {
            return;
        }

        var appUserId = await db.Users.Where(user => user.CustomerId == customer.Id && user.Role == UserRole.Client).Select(user => (int?)user.Id).FirstOrDefaultAsync();
        var title = order.Status switch
        {
            WorkOrderStatus.AwaitingClientApproval => "Заказ-наряд ожидает согласования",
            WorkOrderStatus.Approved => "Заказ-наряд согласован",
            WorkOrderStatus.Delayed => "Работы задерживаются",
            WorkOrderStatus.Completed => "Работы завершены",
            _ => "Заказ-наряд изменен"
        };

        db.Notifications.Add(WorkOrderSupport.CreateCustomerNotification(
            customer,
            title,
            $"Заказ-наряд {order.OrderNumber}: {order.Status.Label()}.",
            order.Status switch
            {
                WorkOrderStatus.AwaitingClientApproval => NotificationKind.WorkOrderApproval,
                WorkOrderStatus.Delayed or WorkOrderStatus.Completed => NotificationKind.WorkStatusChanged,
                _ => NotificationKind.WorkOrderChanged
            },
            order,
            vehicle: await db.Vehicles.FindAsync(order.VehicleId),
            appUserId: appUserId));

        await db.SaveChangesAsync();
    }

    private static void SyncOrderVehicles(WorkOrder order, IReadOnlyCollection<int> vehicleIds)
    {
        var ids = vehicleIds.ToHashSet();
        order.VehiclesInOrder.RemoveAll(link => !ids.Contains(link.VehicleId));
        foreach (var vehicleId in ids)
        {
            if (order.VehiclesInOrder.All(link => link.VehicleId != vehicleId))
            {
                order.VehiclesInOrder.Add(new WorkOrderVehicle { WorkOrder = order, VehicleId = vehicleId });
            }
        }
    }

    private static void SyncAppointmentVehicles(ServiceAppointment appointment, IReadOnlyCollection<int> vehicleIds)
    {
        var ids = vehicleIds.ToHashSet();
        appointment.Vehicles.RemoveAll(link => !ids.Contains(link.VehicleId));
        foreach (var vehicleId in ids)
        {
            if (appointment.Vehicles.All(link => link.VehicleId != vehicleId))
            {
                appointment.Vehicles.Add(new AppointmentVehicle { ServiceAppointment = appointment, VehicleId = vehicleId });
            }
        }
    }

    private static void RecalculateOrderTotal(WorkOrder order)
        => order.TotalCost = order.WorkItems.Sum(item => item.TotalAmount > 0 ? item.TotalAmount : item.Price);

    private void SyncApprovals(WorkOrder order, int currentUserId)
    {
        EnsureApproval(order, currentUserId, ApprovalRole.ServiceManager);
        var clientApproverId = db.Users.Where(user => user.CustomerId == order.CustomerId && user.Role == UserRole.Client).Select(user => (int?)user.Id).FirstOrDefault() ?? currentUserId;
        EnsureApproval(order, clientApproverId, ApprovalRole.Client);
    }

    private static void EnsureApproval(WorkOrder order, int approverUserId, ApprovalRole role)
    {
        if (order.Approvals.All(item => item.ApproverRole != role))
        {
            order.Approvals.Add(new RoleApproval
            {
                ApproverUserId = approverUserId,
                ApproverRole = role,
                Decision = ApprovalDecision.Pending,
                DecisionComment = $"Ожидается решение: {role.Label()}."
            });
        }
    }

    private async Task<ManagerCustomersSectionViewModel> BuildCustomerSectionAsync(string? q, string sort, int? id, int? representativeId, int? vehicleId)
    {
        var query = CustomersBaseQuery();
        if (!string.IsNullOrWhiteSpace(q))
        {
            query = query.Where(customer =>
                customer.Name.Contains(q)
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
        return new ManagerCustomersSectionViewModel
        {
            Customers = await query.ToListAsync(),
            SelectedCustomer = selected,
            SelectedRepresentative = selected?.Representatives.FirstOrDefault(item => item.Id == representativeId),
            SelectedVehicle = selected?.Vehicles.FirstOrDefault(item => item.Id == vehicleId),
            Query = q,
            Sort = sort
        };
    }

    private async Task<ManagerEmployeesSectionViewModel> BuildEmployeeSectionAsync(string? q, string sort, int? id)
    {
        var query = db.Employees.AsNoTracking().AsQueryable();
        if (!string.IsNullOrWhiteSpace(q))
        {
            query = query.Where(employee => employee.FullName.Contains(q) || employee.Position.Contains(q) || employee.Phone.Contains(q) || employee.Email.Contains(q));
        }

        query = sort switch
        {
            "position" => query.OrderBy(employee => employee.Position).ThenBy(employee => employee.FullName),
            "position_desc" => query.OrderByDescending(employee => employee.Position).ThenBy(employee => employee.FullName),
            _ => query.OrderBy(employee => employee.FullName)
        };

        return new ManagerEmployeesSectionViewModel
        {
            Employees = await query.ToListAsync(),
            SelectedEmployee = id is null ? null : await db.Employees.AsNoTracking().FirstOrDefaultAsync(employee => employee.Id == id),
            Query = q,
            Sort = sort
        };
    }

    private async Task<ManagerServicesSectionViewModel> BuildServicesSectionAsync(string? q, string sort, string? category, int? id)
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

        return new ManagerServicesSectionViewModel
        {
            Services = await query.ToListAsync(),
            SelectedService = id is null ? null : await db.Services.Include(service => service.ServiceCategory).AsNoTracking().FirstOrDefaultAsync(service => service.Id == id),
            Query = q,
            Sort = sort,
            Category = category,
            Categories = await db.Services.Select(service => service.Category).Where(value => value != string.Empty).Distinct().OrderBy(value => value).ToListAsync()
        };
    }

    private IQueryable<Customer> CustomersBaseQuery() => db.Customers
        .Include(customer => customer.IndividualProfile)
        .Include(customer => customer.LegalProfile)
        .Include(customer => customer.Representatives.OrderBy(item => item.FullName))
        .Include(customer => customer.Vehicles.OrderBy(item => item.PlateNumber))
        .Include(customer => customer.WorkOrders);

    private IQueryable<DiagnosticConclusion> DiagnosticsBaseQuery() => db.DiagnosticConclusions
        .Include(item => item.Customer).ThenInclude(customer => customer!.Representatives)
        .Include(item => item.Vehicle)
        .Include(item => item.ReceiverEmployee)
        .Include(item => item.WorkOrder);

    private IQueryable<WorkOrder> OrdersBaseQuery() => db.WorkOrders
        .Include(order => order.Customer).ThenInclude(customer => customer!.IndividualProfile)
        .Include(order => order.Customer).ThenInclude(customer => customer!.LegalProfile)
        .Include(order => order.Customer).ThenInclude(customer => customer!.Representatives)
        .Include(order => order.Vehicle)
        .Include(order => order.VehiclesInOrder).ThenInclude(link => link.Vehicle)
        .Include(order => order.ServiceCenter)
        .Include(order => order.ReceiverEmployee)
        .Include(order => order.ServiceManagerEmployee)
        .Include(order => order.ServiceAppointment).ThenInclude(appointment => appointment!.Services)
        .Include(order => order.ServiceAppointment).ThenInclude(appointment => appointment!.Vehicles).ThenInclude(link => link.Vehicle)
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

    private async Task<int> GetCurrentEmployeeIdAsync()
    {
        var userId = await GetCurrentUserIdAsync();
        var employeeId = await db.Users.Where(user => user.Id == userId).Select(user => user.EmployeeId).SingleAsync();
        return employeeId ?? throw new InvalidOperationException("Пользователь не связан с сотрудником.");
    }

    private Task<int> GetCurrentUserIdAsync()
        => Task.FromResult(int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0"));
}
