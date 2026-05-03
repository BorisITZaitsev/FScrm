using FitServiceCRM.Data;
using FitServiceCRM.Models;
using FitServiceCRM.Services;
using FitServiceCRM.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FitServiceCRM.Controllers;

[Authorize(Roles = nameof(UserRole.Admin))]
public sealed class AdminController(AppDbContext db) : Controller
{
    public async Task<IActionResult> Index()
    {
        var model = new AdminDashboardViewModel
        {
            UsersTotal = await db.Users.CountAsync(),
            EmployeesTotal = await db.Employees.CountAsync(),
            CustomersTotal = await db.Customers.CountAsync(),
            ServicesTotal = await db.Services.CountAsync(),
            OrdersTotal = await db.WorkOrders.CountAsync(),
            Users = await UsersBaseQuery()
                .OrderBy(user => user.Login)
                .Take(4)
                .ToListAsync(),
            Employees = await db.Employees
                .OrderBy(employee => employee.FullName)
                .Take(4)
                .ToListAsync(),
            Customers = await db.Customers
                .Include(customer => customer.Vehicles)
                .OrderBy(customer => customer.Name)
                .Take(4)
                .ToListAsync(),
            Services = await db.Services
                .OrderBy(service => service.Name)
                .Take(4)
                .ToListAsync(),
            Orders = await OrdersBaseQuery()
                .OrderByDescending(order => order.PlannedStartAt)
                .Take(4)
                .ToListAsync()
        };

        return View(model);
    }

    public async Task<IActionResult> Users(string? q, string sort = "login", UserRole? role = null, bool? isActive = null, int? id = null, bool create = false, bool edit = false)
    {
        var query = UsersBaseQuery();

        if (!string.IsNullOrWhiteSpace(q))
        {
            query = query.Where(user =>
                user.Login.Contains(q)
                || (user.Employee != null && user.Employee.FullName.Contains(q))
                || (user.Customer != null && user.Customer.Name.Contains(q)));
        }

        if (role is not null)
        {
            query = query.Where(user => user.Role == role);
        }

        if (isActive is not null)
        {
            query = query.Where(user => user.IsActive == isActive);
        }

        query = sort switch
        {
            "login_desc" => query.OrderByDescending(user => user.Login),
            "role" => query.OrderBy(user => user.Role).ThenBy(user => user.Login),
            "role_desc" => query.OrderByDescending(user => user.Role).ThenBy(user => user.Login),
            "name" => query.OrderBy(user => user.Employee != null ? user.Employee.FullName : user.Customer != null ? user.Customer.Name : user.Login),
            "name_desc" => query.OrderByDescending(user => user.Employee != null ? user.Employee.FullName : user.Customer != null ? user.Customer.Name : user.Login),
            _ => query.OrderBy(user => user.Login)
        };

        var model = new AdminUsersSectionViewModel
        {
            Users = await query.ToListAsync(),
            SelectedUser = id is null ? null : await UsersBaseQuery().FirstOrDefaultAsync(user => user.Id == id),
            Employees = await db.Employees.OrderBy(employee => employee.FullName).ToListAsync(),
            Customers = await db.Customers.OrderBy(customer => customer.Name).ToListAsync(),
            Query = q,
            Sort = sort,
            Role = role,
            IsActive = isActive,
            IsCreate = create,
            IsEdit = create || edit
        };

        return View(model);
    }

    public async Task<IActionResult> Employees(string? q, string sort = "name", string? position = null, int? id = null, bool create = false, bool edit = false)
    {
        var query = db.Employees.AsQueryable();

        if (!string.IsNullOrWhiteSpace(q))
        {
            query = query.Where(employee =>
                employee.FullName.Contains(q)
                || employee.Phone.Contains(q)
                || employee.WorkOrders.Any(link =>
                    link.WorkOrder != null
                    && link.WorkOrder.Vehicle != null
                    && (link.WorkOrder.Vehicle.PlateNumber.Contains(q)
                        || link.WorkOrder.Vehicle.Make.Contains(q)
                        || link.WorkOrder.Vehicle.Model.Contains(q))));
        }

        if (!string.IsNullOrWhiteSpace(position))
        {
            query = query.Where(employee => employee.Position == position);
        }

        query = sort switch
        {
            "name_desc" => query.OrderByDescending(employee => employee.FullName),
            "phone" => query.OrderBy(employee => employee.Phone),
            "phone_desc" => query.OrderByDescending(employee => employee.Phone),
            "position" => query.OrderBy(employee => employee.Position).ThenBy(employee => employee.FullName),
            "position_desc" => query.OrderByDescending(employee => employee.Position).ThenBy(employee => employee.FullName),
            _ => query.OrderBy(employee => employee.FullName)
        };

        var model = new AdminEmployeesSectionViewModel
        {
            Employees = await query.ToListAsync(),
            SelectedEmployee = id is null ? null : await db.Employees.FindAsync(id),
            Query = q,
            Sort = sort,
            Position = position,
            Positions = await db.Employees.Select(employee => employee.Position).Distinct().OrderBy(value => value).ToListAsync(),
            IsCreate = create,
            IsEdit = create || edit
        };

        return View(model);
    }

    public async Task<IActionResult> Customers(
        string? q,
        string sort = "name",
        CustomerType? type = null,
        int? id = null,
        bool create = false,
        bool edit = false,
        int? vehicleId = null,
        bool vehicleCreate = false,
        bool vehicleEdit = false)
    {
        var query = db.Customers
            .Include(customer => customer.Vehicles)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(q))
        {
            query = query.Where(customer =>
                customer.Name.Contains(q)
                || customer.Phone.Contains(q)
                || customer.Vehicles.Any(vehicle =>
                    vehicle.PlateNumber.Contains(q)
                    || vehicle.Make.Contains(q)
                    || vehicle.Model.Contains(q)));
        }

        if (type is not null)
        {
            query = query.Where(customer => customer.Type == type);
        }

        query = sort switch
        {
            "name_desc" => query.OrderByDescending(customer => customer.Name),
            "phone" => query.OrderBy(customer => customer.Phone),
            "phone_desc" => query.OrderByDescending(customer => customer.Phone),
            "type" => query.OrderBy(customer => customer.Type).ThenBy(customer => customer.Name),
            "type_desc" => query.OrderByDescending(customer => customer.Type).ThenBy(customer => customer.Name),
            _ => query.OrderBy(customer => customer.Name)
        };

        var selectedCustomer = id is null
            ? null
            : await db.Customers
                .Include(customer => customer.Vehicles.OrderBy(vehicle => vehicle.PlateNumber))
                .FirstOrDefaultAsync(customer => customer.Id == id);

        var selectedVehicle = edit && selectedCustomer is not null && vehicleId is not null
            ? await db.Vehicles.FirstOrDefaultAsync(vehicle => vehicle.Id == vehicleId && vehicle.CustomerId == selectedCustomer.Id)
            : null;

        var model = new AdminCustomersSectionViewModel
        {
            Customers = await query.ToListAsync(),
            SelectedCustomer = selectedCustomer,
            SelectedVehicle = selectedVehicle,
            Query = q,
            Sort = sort,
            Type = type,
            IsCreate = create,
            IsEdit = create || edit,
            IsVehicleCreate = edit && selectedCustomer is not null && vehicleCreate,
            IsVehicleEdit = edit && selectedCustomer is not null && (vehicleCreate || selectedVehicle is not null && vehicleEdit)
        };

        return View(model);
    }

    public async Task<IActionResult> Services(string? q, string sort = "name", string? category = null, string? type = null, int? id = null, bool create = false, bool edit = false)
    {
        var query = db.Services.AsQueryable();

        if (!string.IsNullOrWhiteSpace(q))
        {
            query = query.Where(service => service.Name.Contains(q));
        }

        if (!string.IsNullOrWhiteSpace(category))
        {
            query = query.Where(service => service.Category == category);
        }

        if (!string.IsNullOrWhiteSpace(type))
        {
            query = query.Where(service => service.Type == type);
        }

        query = sort switch
        {
            "name_desc" => query.OrderByDescending(service => service.Name),
            "category" => query.OrderBy(service => service.Category).ThenBy(service => service.Name),
            "category_desc" => query.OrderByDescending(service => service.Category).ThenBy(service => service.Name),
            "type" => query.OrderBy(service => service.Type).ThenBy(service => service.Name),
            "type_desc" => query.OrderByDescending(service => service.Type).ThenBy(service => service.Name),
            "price" => query.OrderBy(service => service.Price),
            "price_desc" => query.OrderByDescending(service => service.Price),
            _ => query.OrderBy(service => service.Name)
        };

        var model = new AdminServicesSectionViewModel
        {
            Services = await query.ToListAsync(),
            SelectedService = id is null ? null : await db.Services.FindAsync(id),
            Query = q,
            Sort = sort,
            Category = category,
            Type = type,
            Categories = await db.Services.Select(service => service.Category).Distinct().OrderBy(value => value).ToListAsync(),
            Types = await db.Services.Select(service => service.Type).Distinct().OrderBy(value => value).ToListAsync(),
            IsCreate = create,
            IsEdit = create || edit
        };

        return View(model);
    }

    public async Task<IActionResult> Orders(string? q, string sort = "date_desc", WorkOrderStatus? status = null, int? serviceId = null, int? id = null, bool create = false, bool edit = false)
    {
        var query = OrdersBaseQuery();

        if (!string.IsNullOrWhiteSpace(q))
        {
            var hasDate = DateTime.TryParse(q, out var date);
            var from = hasDate ? date.Date : DateTime.MinValue;
            var to = hasDate ? from.AddDays(1) : DateTime.MinValue;

            query = query.Where(order =>
                (order.Customer != null && order.Customer.Name.Contains(q))
                || (order.ReceiverEmployee != null && order.ReceiverEmployee.FullName.Contains(q))
                || (order.Vehicle != null && (order.Vehicle.PlateNumber.Contains(q) || order.Vehicle.Make.Contains(q) || order.Vehicle.Model.Contains(q)))
                || (hasDate && order.PlannedStartAt >= from && order.PlannedStartAt < to));
        }

        if (status is not null)
        {
            query = query.Where(order => order.Status == status);
        }

        if (serviceId is not null)
        {
            query = query.Where(order => order.OrderServices.Any(orderService => orderService.ServiceItemId == serviceId));
        }

        query = sort switch
        {
            "date" => query.OrderBy(order => order.PlannedStartAt),
            "client" => query.OrderBy(order => order.Customer != null ? order.Customer.Name : string.Empty).ThenByDescending(order => order.PlannedStartAt),
            "client_desc" => query.OrderByDescending(order => order.Customer != null ? order.Customer.Name : string.Empty).ThenByDescending(order => order.PlannedStartAt),
            "master" => query.OrderBy(order => order.ReceiverEmployee != null ? order.ReceiverEmployee.FullName : string.Empty),
            "master_desc" => query.OrderByDescending(order => order.ReceiverEmployee != null ? order.ReceiverEmployee.FullName : string.Empty),
            "status" => query.OrderBy(order => order.Status).ThenByDescending(order => order.PlannedStartAt),
            "status_desc" => query.OrderByDescending(order => order.Status).ThenByDescending(order => order.PlannedStartAt),
            "total" => query.OrderBy(order => order.TotalCost),
            "total_desc" => query.OrderByDescending(order => order.TotalCost),
            _ => query.OrderByDescending(order => order.PlannedStartAt)
        };

        var model = new AdminOrdersSectionViewModel
        {
            Orders = await query.ToListAsync(),
            SelectedOrder = id is null ? null : await OrdersBaseQuery().FirstOrDefaultAsync(order => order.Id == id),
            Customers = await db.Customers.OrderBy(customer => customer.Name).ToListAsync(),
            Vehicles = await db.Vehicles.Include(vehicle => vehicle.Customer).OrderBy(vehicle => vehicle.PlateNumber).ToListAsync(),
            Employees = await db.Employees.OrderBy(employee => employee.FullName).ToListAsync(),
            Services = await db.Services.OrderBy(service => service.Name).ToListAsync(),
            Query = q,
            Sort = sort,
            Status = status,
            ServiceId = serviceId,
            IsCreate = create,
            IsEdit = create || edit
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveUser(int? id, string login, string? password, UserRole role, bool isActive, int? employeeId, int? customerId)
    {
        var duplicateExists = await db.Users.AnyAsync(user => user.Login == login && user.Id != id);
        if (duplicateExists)
        {
            TempData["Message"] = "Пользователь с таким логином уже существует.";
            return RedirectToAction(nameof(Users), new { id, edit = id is not null, create = id is null });
        }

        AppUser user;
        if (id is null)
        {
            user = new AppUser();
            db.Users.Add(user);
        }
        else
        {
            user = await db.Users.FindAsync(id) ?? throw new InvalidOperationException("Пользователь не найден.");
        }

        user.Login = login;
        user.Role = role;
        user.IsActive = isActive;
        user.EmployeeId = role == UserRole.Client ? null : employeeId;
        user.CustomerId = role == UserRole.Client ? customerId : null;

        if (id is null || !string.IsNullOrWhiteSpace(password))
        {
            user.PasswordHash = PasswordService.Hash(password ?? "ChangeMe2026!");
        }

        await db.SaveChangesAsync();
        TempData["Message"] = id is null ? "Пользователь создан." : "Пользователь сохранен.";
        return RedirectToAction(nameof(Users), new { id = user.Id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveEmployee(int? id, string fullName, string phone, string email, string position)
    {
        Employee employee;
        if (id is null)
        {
            employee = new Employee();
            db.Employees.Add(employee);
        }
        else
        {
            employee = await db.Employees.FindAsync(id) ?? throw new InvalidOperationException("Сотрудник не найден.");
        }

        employee.FullName = fullName;
        employee.Phone = phone;
        employee.Email = email;
        employee.Position = position;

        await db.SaveChangesAsync();
        TempData["Message"] = id is null ? "Сотрудник создан." : "Сотрудник сохранен.";
        return RedirectToAction(nameof(Employees), new { id = employee.Id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveCustomer(int? id, CustomerType type, string name, string phone, string email, string address, string taxNumber)
    {
        Customer customer;
        if (id is null)
        {
            customer = new Customer();
            db.Customers.Add(customer);
        }
        else
        {
            customer = await db.Customers.FindAsync(id) ?? throw new InvalidOperationException("Клиент не найден.");
        }

        customer.Type = type;
        customer.Name = name;
        customer.Phone = phone;
        customer.Email = email;
        customer.Address = address;
        customer.TaxNumber = taxNumber;

        await db.SaveChangesAsync();
        TempData["Message"] = id is null ? "Клиент создан." : "Клиент сохранен.";
        return RedirectToAction(nameof(Customers), new { id = customer.Id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveVehicle(int? id, int customerId, string make, string model, string plateNumber, int year, int mileage)
    {
        var customerExists = await db.Customers.AnyAsync(customer => customer.Id == customerId);
        if (!customerExists)
        {
            TempData["Message"] = "Клиент для автомобиля не найден.";
            return RedirectToAction(nameof(Customers));
        }

        Vehicle vehicle;
        if (id is null)
        {
            vehicle = new Vehicle { CustomerId = customerId };
            db.Vehicles.Add(vehicle);
        }
        else
        {
            vehicle = await db.Vehicles.FirstOrDefaultAsync(candidate => candidate.Id == id && candidate.CustomerId == customerId)
                ?? throw new InvalidOperationException("Автомобиль не найден.");
        }

        vehicle.Make = make;
        vehicle.Model = model;
        vehicle.PlateNumber = plateNumber;
        vehicle.Year = year;
        vehicle.Mileage = mileage;

        await db.SaveChangesAsync();
        TempData["Message"] = id is null ? "Автомобиль добавлен." : "Автомобиль сохранен.";
        return RedirectToAction(nameof(Customers), new { id = customerId, edit = true, vehicleId = vehicle.Id, vehicleEdit = true });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteVehicle(int id, int customerId)
    {
        var vehicle = await db.Vehicles
            .Include(candidate => candidate.WorkOrders)
            .Include(candidate => candidate.WorkItems)
            .FirstOrDefaultAsync(candidate => candidate.Id == id && candidate.CustomerId == customerId);

        if (vehicle is null)
        {
            TempData["Message"] = "Автомобиль не найден.";
            return RedirectToAction(nameof(Customers), new { id = customerId, edit = true });
        }

        if (vehicle.WorkOrders.Count > 0 || vehicle.WorkItems.Count > 0)
        {
            TempData["Message"] = "Автомобиль нельзя удалить: он уже связан с заказ-нарядами или работами.";
            return RedirectToAction(nameof(Customers), new { id = customerId, edit = true, vehicleId = vehicle.Id, vehicleEdit = true });
        }

        db.Vehicles.Remove(vehicle);
        await db.SaveChangesAsync();
        TempData["Message"] = "Автомобиль удален.";
        return RedirectToAction(nameof(Customers), new { id = customerId, edit = true });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveService(int? id, string name, string category, string type, int duration, decimal price, int intervalDays)
    {
        ServiceItem service;
        if (id is null)
        {
            service = new ServiceItem();
            db.Services.Add(service);
        }
        else
        {
            service = await db.Services.FindAsync(id) ?? throw new InvalidOperationException("Услуга не найдена.");
        }

        service.Name = name;
        service.Category = category;
        service.Type = type;
        service.EstimatedDurationMinutes = duration;
        service.Price = price;
        service.ApproximatePurchaseIntervalDays = intervalDays;

        await db.SaveChangesAsync();
        TempData["Message"] = id is null ? "Услуга создана." : "Услуга сохранена.";
        return RedirectToAction(nameof(Services), new { id = service.Id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveOrder(
        int? id,
        int customerId,
        int vehicleId,
        int receiverEmployeeId,
        WorkOrderStatus status,
        DateTime plannedStartAt,
        DateTime? completedAt,
        int plannedDurationMinutes,
        int? actualDurationMinutes,
        decimal totalCost,
        string customerComment,
        string internalComment,
        int serviceId,
        int? executorEmployeeId)
    {
        WorkOrder order;
        if (id is null)
        {
            order = new WorkOrder { CreatedAt = DateTime.Now };
            db.WorkOrders.Add(order);
        }
        else
        {
            order = await db.WorkOrders
                .Include(candidate => candidate.OrderServices)
                .Include(candidate => candidate.Executors)
                .FirstOrDefaultAsync(candidate => candidate.Id == id)
                ?? throw new InvalidOperationException("Заказ-наряд не найден.");
        }

        order.CustomerId = customerId;
        order.VehicleId = vehicleId;
        order.ReceiverEmployeeId = receiverEmployeeId;
        order.Status = status;
        order.PlannedStartAt = plannedStartAt;
        order.CompletedAt = completedAt;
        order.PlannedDurationMinutes = plannedDurationMinutes;
        order.ActualDurationMinutes = actualDurationMinutes;
        order.TotalCost = totalCost;
        order.CustomerComment = customerComment;
        order.InternalComment = internalComment;

        order.OrderServices.Clear();
        order.OrderServices.Add(new OrderService { WorkOrder = order, ServiceItemId = serviceId });

        order.Executors.Clear();
        if (executorEmployeeId is not null)
        {
            order.Executors.Add(new WorkOrderEmployee { WorkOrder = order, EmployeeId = executorEmployeeId.Value });
        }

        if (id is null)
        {
            order.WorkItems.Add(new WorkItem
            {
                ServiceItemId = serviceId,
                VehicleId = vehicleId,
                Description = "Первичная работа по заказ-наряду",
                Price = totalCost
            });
        }

        await db.SaveChangesAsync();
        TempData["Message"] = id is null ? "Заказ-наряд создан." : "Заказ-наряд сохранен.";
        return RedirectToAction(nameof(Orders), new { id = order.Id });
    }

    private IQueryable<AppUser> UsersBaseQuery() => db.Users
        .Include(user => user.Employee)
        .Include(user => user.Customer);

    private IQueryable<WorkOrder> OrdersBaseQuery() => db.WorkOrders
        .Include(order => order.Customer)
        .Include(order => order.Vehicle)
        .Include(order => order.ReceiverEmployee)
        .Include(order => order.OrderServices).ThenInclude(orderService => orderService.ServiceItem)
        .Include(order => order.Executors).ThenInclude(executor => executor.Employee)
        .Include(order => order.WorkItems).ThenInclude(work => work.ServiceItem)
        .Include(order => order.DiagnosticDocuments)
        .Include(order => order.SupportDocuments);
}
