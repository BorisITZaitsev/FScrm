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
            ServiceCentersTotal = await db.ServiceCenters.CountAsync(),
            UsersTotal = await db.Users.CountAsync(),
            EmployeesTotal = await db.Employees.CountAsync(),
            CustomersTotal = await db.Customers.CountAsync(),
            ServicesTotal = await db.Services.CountAsync(),
            OrdersTotal = await db.WorkOrders.CountAsync(),
            ServiceCenters = await db.ServiceCenters
                .OrderBy(center => center.Name)
                .Take(4)
                .ToListAsync(),
            Users = await UsersBaseQuery()
                .OrderBy(user => user.Login)
                .Take(4)
                .ToListAsync(),
            Employees = await db.Employees
                .OrderBy(employee => employee.FullName)
                .Take(4)
                .ToListAsync(),
            Customers = await CustomersBaseQuery()
                .OrderBy(customer => customer.Name)
                .Take(4)
                .ToListAsync(),
            Services = await db.Services
                .Include(service => service.ServiceCategory)
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
            Roles = await db.Roles.OrderBy(roleEntity => roleEntity.Name).ToListAsync(),
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
        var query = db.Employees
            .Include(employee => employee.User)
            .AsQueryable();

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
            SelectedEmployee = id is null ? null : await db.Employees.Include(employee => employee.User).FirstOrDefaultAsync(employee => employee.Id == id),
            Users = await db.Users.OrderBy(user => user.Login).ToListAsync(),
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
        CustomerStatus? status = null,
        int? id = null,
        bool create = false,
        bool edit = false,
        int? vehicleId = null,
        bool vehicleCreate = false,
        bool vehicleEdit = false,
        int? representativeId = null,
        bool representativeCreate = false,
        bool representativeEdit = false)
    {
        var query = CustomersBaseQuery();

        if (!string.IsNullOrWhiteSpace(q))
        {
            query = query.Where(customer =>
                customer.Name.Contains(q)
                || customer.Phone.Contains(q)
                || (customer.IndividualProfile != null && customer.IndividualProfile.FullName.Contains(q))
                || (customer.LegalProfile != null && customer.LegalProfile.OrganizationName.Contains(q))
                || customer.Representatives.Any(representative => representative.FullName.Contains(q))
                || customer.Vehicles.Any(vehicle =>
                    vehicle.PlateNumber.Contains(q)
                    || vehicle.Vin.Contains(q)
                    || vehicle.Make.Contains(q)
                    || vehicle.Model.Contains(q)));
        }

        if (type is not null)
        {
            query = query.Where(customer => customer.Type == type);
        }

        if (status is not null)
        {
            query = query.Where(customer => customer.Status == status);
        }

        query = sort switch
        {
            "name_desc" => query.OrderByDescending(customer => customer.Name),
            "phone" => query.OrderBy(customer => customer.Phone),
            "phone_desc" => query.OrderByDescending(customer => customer.Phone),
            "type" => query.OrderBy(customer => customer.Type).ThenBy(customer => customer.Name),
            "type_desc" => query.OrderByDescending(customer => customer.Type).ThenBy(customer => customer.Name),
            "status" => query.OrderBy(customer => customer.Status).ThenBy(customer => customer.Name),
            "status_desc" => query.OrderByDescending(customer => customer.Status).ThenBy(customer => customer.Name),
            _ => query.OrderBy(customer => customer.Name)
        };

        var selectedCustomer = id is null
            ? null
            : await CustomersBaseQuery()
                .FirstOrDefaultAsync(customer => customer.Id == id);

        var selectedVehicle = edit && selectedCustomer is not null && vehicleId is not null
            ? await db.Vehicles.FirstOrDefaultAsync(vehicle => vehicle.Id == vehicleId && vehicle.CustomerId == selectedCustomer.Id)
            : null;

        var selectedRepresentative = edit && selectedCustomer is not null && representativeId is not null
            ? await db.CustomerRepresentatives.FirstOrDefaultAsync(representative => representative.Id == representativeId && representative.CustomerId == selectedCustomer.Id)
            : null;

        var model = new AdminCustomersSectionViewModel
        {
            Customers = await query.ToListAsync(),
            SelectedCustomer = selectedCustomer,
            SelectedVehicle = selectedVehicle,
            SelectedRepresentative = selectedRepresentative,
            Query = q,
            Sort = sort,
            Type = type,
            Status = status,
            IsCreate = create,
            IsEdit = create || edit,
            IsVehicleCreate = edit && selectedCustomer is not null && vehicleCreate,
            IsVehicleEdit = edit && selectedCustomer is not null && (vehicleCreate || selectedVehicle is not null && vehicleEdit),
            IsRepresentativeCreate = edit && selectedCustomer is not null && representativeCreate,
            IsRepresentativeEdit = edit && selectedCustomer is not null && (representativeCreate || selectedRepresentative is not null && representativeEdit)
        };

        return View(model);
    }

    public async Task<IActionResult> Services(string? q, string sort = "name", string? category = null, string? type = null, int? id = null, bool create = false, bool edit = false)
    {
        var query = db.Services
            .Include(service => service.ServiceCategory)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(q))
        {
            query = query.Where(service => service.Name.Contains(q) || service.Description.Contains(q));
        }

        if (!string.IsNullOrWhiteSpace(category))
        {
            query = query.Where(service => service.Category == category || (service.ServiceCategory != null && service.ServiceCategory.Name == category));
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
            "price" => query.OrderBy(service => service.BasePrice),
            "price_desc" => query.OrderByDescending(service => service.BasePrice),
            _ => query.OrderBy(service => service.Name)
        };

        var model = new AdminServicesSectionViewModel
        {
            Services = await query.ToListAsync(),
            SelectedService = id is null ? null : await db.Services.Include(service => service.ServiceCategory).FirstOrDefaultAsync(service => service.Id == id),
            ServiceCategories = await db.ServiceCategories.OrderBy(categoryEntity => categoryEntity.Name).ToListAsync(),
            Query = q,
            Sort = sort,
            Category = category,
            Type = type,
            Categories = await db.Services.Select(service => service.Category).Where(value => value != string.Empty).Distinct().OrderBy(value => value).ToListAsync(),
            Types = await db.Services.Select(service => service.Type).Where(value => value != string.Empty).Distinct().OrderBy(value => value).ToListAsync(),
            IsCreate = create,
            IsEdit = create || edit
        };

        return View(model);
    }

    public async Task<IActionResult> Orders(string? q, string sort = "date_desc", WorkOrderStatus? status = null, int? serviceId = null, int? id = null, bool create = false, bool edit = false, int? workItemId = null, bool workItemCreate = false, bool workItemEdit = false)
    {
        var query = OrdersBaseQuery();

        if (!string.IsNullOrWhiteSpace(q))
        {
            var hasDate = DateTime.TryParse(q, out var date);
            var from = hasDate ? date.Date : DateTime.MinValue;
            var to = hasDate ? from.AddDays(1) : DateTime.MinValue;

            query = query.Where(order =>
                order.OrderNumber.Contains(q)
                || (order.Customer != null && order.Customer.Name.Contains(q))
                || (order.ReceiverEmployee != null && order.ReceiverEmployee.FullName.Contains(q))
                || (order.ServiceManagerEmployee != null && order.ServiceManagerEmployee.FullName.Contains(q))
                || (order.ServiceCenter != null && order.ServiceCenter.Name.Contains(q))
                || (order.Vehicle != null && (order.Vehicle.PlateNumber.Contains(q) || order.Vehicle.Make.Contains(q) || order.Vehicle.Model.Contains(q)))
                || order.VehiclesInOrder.Any(link => link.Vehicle != null && (link.Vehicle.PlateNumber.Contains(q) || link.Vehicle.Make.Contains(q) || link.Vehicle.Model.Contains(q)))
                || (hasDate && order.PlannedStartAt >= from && order.PlannedStartAt < to));
        }

        if (status is not null)
        {
            query = query.Where(order => order.Status == status);
        }

        if (serviceId is not null)
        {
            query = query.Where(order =>
                order.OrderServices.Any(orderService => orderService.ServiceItemId == serviceId)
                || order.WorkItems.Any(work => work.ServiceItemId == serviceId));
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
            SelectedWorkItem = edit && id is not null && workItemId is not null
                ? await db.WorkItems.Include(item => item.ServiceItem).Include(item => item.Vehicle).FirstOrDefaultAsync(item => item.Id == workItemId && item.WorkOrderId == id)
                : null,
            ServiceCenters = await db.ServiceCenters.OrderBy(center => center.Name).ToListAsync(),
            Customers = await db.Customers.OrderBy(customer => customer.Name).ToListAsync(),
            Vehicles = await db.Vehicles.Include(vehicle => vehicle.Customer).OrderBy(vehicle => vehicle.PlateNumber).ToListAsync(),
            Employees = await db.Employees.OrderBy(employee => employee.FullName).ToListAsync(),
            Services = await db.Services.Include(service => service.ServiceCategory).OrderBy(service => service.Name).ToListAsync(),
            Query = q,
            Sort = sort,
            Status = status,
            ServiceId = serviceId,
            IsCreate = create,
            IsEdit = create || edit,
            IsWorkItemCreate = edit && id is not null && workItemCreate,
            IsWorkItemEdit = edit && id is not null && (workItemCreate || workItemId is not null && workItemEdit)
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
            user = await db.Users
                .Include(candidate => candidate.RoleAssignments)
                .FirstOrDefaultAsync(candidate => candidate.Id == id)
                ?? throw new InvalidOperationException("Пользователь не найден.");
        }

        user.Login = login.Trim();
        user.Role = role;
        user.IsActive = isActive;
        user.EmployeeId = role == UserRole.Client ? null : employeeId;
        user.CustomerId = role == UserRole.Client ? customerId : null;

        if (id is null || !string.IsNullOrWhiteSpace(password))
        {
            user.PasswordHash = PasswordService.Hash(password ?? "ChangeMe2026!");
        }

        user.RoleAssignments.Clear();
        var systemRole = await EnsureRoleAsync(role);
        user.RoleAssignments.Add(new AppUserRole { AppUser = user, SystemRole = systemRole });

        await db.SaveChangesAsync();

        await db.Employees.Where(employee => employee.UserId == user.Id && employee.Id != employeeId)
            .ExecuteUpdateAsync(setters => setters.SetProperty(employee => employee.UserId, (int?)null));

        if (employeeId is not null)
        {
            await db.Employees.Where(employee => employee.Id == employeeId)
                .ExecuteUpdateAsync(setters => setters.SetProperty(employee => employee.UserId, user.Id));
        }

        await SyncCustomerAccessAsync(user);

        TempData["Message"] = id is null ? "Пользователь создан." : "Пользователь сохранен.";
        return RedirectToAction(nameof(Users), new { id = user.Id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveEmployee(int? id, int? userId, string fullName, string phone, string email, string position)
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

        employee.UserId = userId;
        employee.FullName = fullName.Trim();
        employee.Phone = phone.Trim();
        employee.Email = email.Trim();
        employee.Position = position.Trim();

        await db.SaveChangesAsync();
        TempData["Message"] = id is null ? "Сотрудник создан." : "Сотрудник сохранен.";
        return RedirectToAction(nameof(Employees), new { id = employee.Id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveCustomer(
        int? id,
        CustomerType type,
        CustomerStatus status,
        string? comment,
        string? individualFullName,
        DateTime? birthDate,
        string? individualPhone,
        string? individualEmail,
        string? organizationName,
        string? inn,
        string? kpp,
        string? ogrn,
        string? legalAddress,
        string? actualAddress,
        string? organizationPhone,
        string? organizationEmail)
    {
        Customer customer;
        if (id is null)
        {
            customer = new Customer();
            db.Customers.Add(customer);
        }
        else
        {
            customer = await db.Customers
                .Include(candidate => candidate.IndividualProfile)
                .Include(candidate => candidate.LegalProfile)
                .Include(candidate => candidate.Representatives)
                .FirstOrDefaultAsync(candidate => candidate.Id == id)
                ?? throw new InvalidOperationException("Клиент не найден.");
        }

        customer.Type = type;
        customer.Status = status;
        customer.Comment = comment?.Trim() ?? string.Empty;

        if (type == CustomerType.Individual)
        {
            customer.IndividualProfile ??= new IndividualCustomer { Customer = customer };
            customer.IndividualProfile.FullName = individualFullName?.Trim() ?? string.Empty;
            customer.IndividualProfile.BirthDate = birthDate is null ? null : DateOnly.FromDateTime(birthDate.Value);
            customer.IndividualProfile.Phone = individualPhone?.Trim() ?? string.Empty;
            customer.IndividualProfile.Email = individualEmail?.Trim() ?? string.Empty;

            if (customer.LegalProfile is not null)
            {
                db.LegalCustomers.Remove(customer.LegalProfile);
            }

            customer.Name = customer.IndividualProfile.FullName;
            customer.Phone = customer.IndividualProfile.Phone;
            customer.Email = customer.IndividualProfile.Email;
            customer.Address = string.Empty;
            customer.TaxNumber = string.Empty;
        }
        else
        {
            customer.LegalProfile ??= new LegalCustomer { Customer = customer };
            customer.LegalProfile.OrganizationName = organizationName?.Trim() ?? string.Empty;
            customer.LegalProfile.Inn = inn?.Trim() ?? string.Empty;
            customer.LegalProfile.Kpp = kpp?.Trim() ?? string.Empty;
            customer.LegalProfile.Ogrn = ogrn?.Trim() ?? string.Empty;
            customer.LegalProfile.LegalAddress = legalAddress?.Trim() ?? string.Empty;
            customer.LegalProfile.ActualAddress = actualAddress?.Trim() ?? string.Empty;
            customer.LegalProfile.Phone = organizationPhone?.Trim() ?? string.Empty;
            customer.LegalProfile.Email = organizationEmail?.Trim() ?? string.Empty;

            if (customer.IndividualProfile is not null)
            {
                db.IndividualCustomers.Remove(customer.IndividualProfile);
            }

            customer.Name = customer.LegalProfile.OrganizationName;
            customer.Phone = customer.LegalProfile.Phone;
            customer.Email = customer.LegalProfile.Email;
            customer.Address = customer.LegalProfile.ActualAddress;
            customer.TaxNumber = customer.LegalProfile.Inn;
        }

        await db.SaveChangesAsync();
        TempData["Message"] = id is null ? "Клиент создан." : "Клиент сохранен.";
        return RedirectToAction(nameof(Customers), new { id = customer.Id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveRepresentative(int? id, int customerId, string fullName, string? position, string? phone, string? email)
    {
        var customer = await db.Customers
            .Include(candidate => candidate.LegalProfile)
            .FirstOrDefaultAsync(candidate => candidate.Id == customerId)
            ?? throw new InvalidOperationException("Клиент не найден.");

        if (customer.Type != CustomerType.Company)
        {
            TempData["Message"] = "Представителей можно добавлять только для юридического лица.";
            return RedirectToAction(nameof(Customers), new { id = customerId, edit = true });
        }

        CustomerRepresentative representative;
        if (id is null)
        {
            representative = new CustomerRepresentative { CustomerId = customerId };
            db.CustomerRepresentatives.Add(representative);
        }
        else
        {
            representative = await db.CustomerRepresentatives.FirstOrDefaultAsync(candidate => candidate.Id == id && candidate.CustomerId == customerId)
                ?? throw new InvalidOperationException("Представитель не найден.");
        }

        representative.FullName = fullName.Trim();
        representative.Position = position?.Trim() ?? string.Empty;
        representative.Phone = phone?.Trim() ?? string.Empty;
        representative.Email = email?.Trim() ?? string.Empty;

        await db.SaveChangesAsync();
        TempData["Message"] = id is null ? "Представитель добавлен." : "Представитель сохранен.";
        return RedirectToAction(nameof(Customers), new { id = customerId, edit = true, representativeId = representative.Id, representativeEdit = true });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveVehicle(int? id, int customerId, string? vin, string make, string model, string plateNumber, int year, int mileage, string? color)
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

        vehicle.Vin = vin?.Trim() ?? string.Empty;
        vehicle.Make = make.Trim();
        vehicle.Model = model.Trim();
        vehicle.PlateNumber = plateNumber.Trim();
        vehicle.Year = year;
        vehicle.Mileage = mileage;
        vehicle.Color = color?.Trim() ?? string.Empty;

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
            .Include(candidate => candidate.Appointments)
            .FirstOrDefaultAsync(candidate => candidate.Id == id && candidate.CustomerId == customerId);

        if (vehicle is null)
        {
            TempData["Message"] = "Автомобиль не найден.";
            return RedirectToAction(nameof(Customers), new { id = customerId, edit = true });
        }

        if (vehicle.WorkOrders.Count > 0 || vehicle.WorkItems.Count > 0 || vehicle.Appointments.Count > 0)
        {
            TempData["Message"] = "Автомобиль нельзя удалить: он уже связан с заказ-нарядами, работами или записями.";
            return RedirectToAction(nameof(Customers), new { id = customerId, edit = true, vehicleId = vehicle.Id, vehicleEdit = true });
        }

        db.Vehicles.Remove(vehicle);
        await db.SaveChangesAsync();
        TempData["Message"] = "Автомобиль удален.";
        return RedirectToAction(nameof(Customers), new { id = customerId, edit = true });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteRepresentative(int id, int customerId)
    {
        var representative = await db.CustomerRepresentatives
            .Include(candidate => candidate.SystemAccesses)
            .FirstOrDefaultAsync(candidate => candidate.Id == id && candidate.CustomerId == customerId);

        if (representative is null)
        {
            TempData["Message"] = "Представитель не найден.";
            return RedirectToAction(nameof(Customers), new { id = customerId, edit = true });
        }

        if (representative.SystemAccesses.Count > 0)
        {
            TempData["Message"] = "Представителя нельзя удалить: он связан с доступом в систему.";
            return RedirectToAction(nameof(Customers), new { id = customerId, edit = true, representativeId = representative.Id, representativeEdit = true });
        }

        db.CustomerRepresentatives.Remove(representative);
        await db.SaveChangesAsync();
        TempData["Message"] = "Представитель удален.";
        return RedirectToAction(nameof(Customers), new { id = customerId, edit = true });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveService(int? id, string name, string categoryName, string? type, string? description, int duration, decimal price, int intervalDays)
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

        var category = await EnsureServiceCategoryAsync(categoryName, description);

        service.Name = name.Trim();
        service.ServiceCategory = category;
        service.Category = category.Name;
        service.Type = type?.Trim() ?? category.Name;
        service.Description = description?.Trim() ?? string.Empty;
        service.StandardDurationMinutes = duration;
        service.EstimatedDurationMinutes = duration;
        service.BasePrice = price;
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
                .Include(candidate => candidate.ServiceAppointment).ThenInclude(appointment => appointment!.Services)
                .Include(candidate => candidate.VehiclesInOrder)
                .Include(candidate => candidate.OrderServices)
                .Include(candidate => candidate.Executors)
                .Include(candidate => candidate.WorkItems)
                .FirstOrDefaultAsync(candidate => candidate.Id == id)
                ?? throw new InvalidOperationException("Заказ-наряд не найден.");
        }

        var service = await db.Services.FindAsync(serviceId) ?? throw new InvalidOperationException("Услуга не найдена.");
        var generatedNumber = $"FS-{DateTime.Now:yyyyMMdd}-{(id ?? await db.WorkOrders.CountAsync() + 1):D4}";
        var linkedVehicleIds = (selectedVehicleIds ?? [])
            .Append(vehicleId)
            .Where(candidate => candidate > 0)
            .Distinct()
            .ToArray();

        if (linkedVehicleIds.Length == 0)
        {
            TempData["Message"] = "Для заказ-наряда нужно выбрать хотя бы один автомобиль.";
            return RedirectToAction(nameof(Orders), new { id, edit = true, create = id is null });
        }

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
        order.CompletedAt = actualEndAt;
        order.PlannedDurationMinutes = plannedEndAt is null ? service.EstimatedDurationMinutes : (int)Math.Max(1, (plannedEndAt.Value - plannedStartAt).TotalMinutes);
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
        order.ServiceAppointment.Status = status == WorkOrderStatus.Canceled ? AppointmentStatus.Canceled : AppointmentStatus.Confirmed;
        order.ServiceAppointment.CustomerComment = order.CustomerComment;
        order.ServiceAppointment.Services.Clear();
        order.ServiceAppointment.Services.Add(new AppointmentService { ServiceItemId = serviceId });

        order.OrderServices.Clear();
        order.OrderServices.Add(new OrderService { WorkOrder = order, ServiceItemId = serviceId });

        order.Executors.Clear();
        if (executorEmployeeId is not null)
        {
            order.Executors.Add(new WorkOrderEmployee { WorkOrder = order, EmployeeId = executorEmployeeId.Value });
        }

        SyncOrderVehicles(order, linkedVehicleIds);

        var workItem = order.WorkItems.FirstOrDefault();
        if (workItem is null)
        {
            workItem = new WorkItem { WorkOrder = order };
            order.WorkItems.Add(workItem);
        }

        workItem.ServiceItemId = serviceId;
        workItem.VehicleId = primaryVehicleId;
        workItem.ExecutorEmployeeId = executorEmployeeId;
        workItem.Description = $"{service.Name}: работа по заказ-наряду {order.OrderNumber}";
        workItem.Quantity = 1;
        workItem.UnitPrice = totalCost;
        workItem.Discount = 0;
        workItem.TotalAmount = totalCost;
        workItem.Price = totalCost;
        workItem.Status = status switch
        {
            WorkOrderStatus.Completed => WorkItemStatus.Completed,
            WorkOrderStatus.AwaitingClientApproval or WorkOrderStatus.AwaitingManagerApproval => WorkItemStatus.AwaitingApproval,
            WorkOrderStatus.Canceled => WorkItemStatus.Canceled,
            _ => WorkItemStatus.InProgress
        };
        workItem.PlannedStartAt = plannedStartAt;
        workItem.PlannedEndAt = plannedEndAt;
        workItem.ActualStartAt = actualStartAt;
        workItem.ActualEndAt = actualEndAt;

        await db.SaveChangesAsync();
        TempData["Message"] = id is null ? "Заказ-наряд создан." : "Заказ-наряд сохранен.";
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
        var order = await db.WorkOrders
            .Include(candidate => candidate.VehiclesInOrder)
            .Include(candidate => candidate.OrderServices)
            .Include(candidate => candidate.WorkItems)
            .FirstOrDefaultAsync(candidate => candidate.Id == orderId)
            ?? throw new InvalidOperationException("Заказ-наряд не найден.");

        var service = await db.Services.FindAsync(serviceId) ?? throw new InvalidOperationException("Услуга не найдена.");

        WorkItem workItem;
        if (id is null)
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

        if (order.VehicleId == 0)
        {
            order.VehicleId = vehicleId;
        }

        RecalculateOrderTotal(order);
        await db.SaveChangesAsync();

        TempData["Message"] = id is null ? "Работа добавлена." : "Работа сохранена.";
        return RedirectToAction(nameof(Orders), new { id = orderId, edit = true, workItemId = workItem.Id, workItemEdit = true });
    }

    private IQueryable<AppUser> UsersBaseQuery() => db.Users
        .Include(user => user.Employee)
        .Include(user => user.Customer)
        .Include(user => user.RoleAssignments).ThenInclude(link => link.SystemRole);

    private IQueryable<Customer> CustomersBaseQuery() => db.Customers
        .Include(customer => customer.IndividualProfile)
        .Include(customer => customer.LegalProfile)
        .Include(customer => customer.Representatives.OrderBy(representative => representative.FullName))
        .Include(customer => customer.Vehicles.OrderBy(vehicle => vehicle.PlateNumber));

    private IQueryable<WorkOrder> OrdersBaseQuery() => db.WorkOrders
        .Include(order => order.Customer)
        .Include(order => order.Vehicle)
        .Include(order => order.VehiclesInOrder).ThenInclude(link => link.Vehicle)
        .Include(order => order.ServiceCenter)
        .Include(order => order.ReceiverEmployee)
        .Include(order => order.ServiceManagerEmployee)
        .Include(order => order.ServiceAppointment).ThenInclude(appointment => appointment!.Services).ThenInclude(link => link.ServiceItem)
        .Include(order => order.OrderServices).ThenInclude(orderService => orderService.ServiceItem)
        .Include(order => order.Executors).ThenInclude(executor => executor.Employee)
        .Include(order => order.WorkItems).ThenInclude(work => work.ServiceItem)
        .Include(order => order.DiagnosticDocuments)
        .Include(order => order.SupportDocuments)
        .Include(order => order.DiagnosticConclusions)
        .Include(order => order.Approvals);

    private static void SyncOrderVehicles(WorkOrder order, IReadOnlyCollection<int> vehicleIds)
    {
        var vehicleIdSet = vehicleIds.ToHashSet();
        order.VehiclesInOrder.RemoveAll(link => !vehicleIdSet.Contains(link.VehicleId));

        foreach (var vehicleId in vehicleIdSet)
        {
            if (order.VehiclesInOrder.All(link => link.VehicleId != vehicleId))
            {
                order.VehiclesInOrder.Add(new WorkOrderVehicle { WorkOrder = order, VehicleId = vehicleId });
            }
        }
    }

    private static void RecalculateOrderTotal(WorkOrder order)
    {
        order.TotalCost = order.WorkItems.Sum(item => item.TotalAmount > 0 ? item.TotalAmount : item.Price);
    }

    private async Task<SystemRole> EnsureRoleAsync(UserRole role)
    {
        var code = role.ToString();
        var systemRole = await db.Roles.FirstOrDefaultAsync(candidate => candidate.Code == code);
        if (systemRole is not null)
        {
            return systemRole;
        }

        systemRole = new SystemRole
        {
            Code = code,
            Name = role.Label(),
            Description = $"Роль {role.Label().ToLowerInvariant()} CRM Fit Service"
        };
        db.Roles.Add(systemRole);
        return systemRole;
    }

    private async Task<ServiceCategory> EnsureServiceCategoryAsync(string categoryName, string? description)
    {
        var normalized = string.IsNullOrWhiteSpace(categoryName) ? "Без категории" : categoryName.Trim();
        var category = await db.ServiceCategories.FirstOrDefaultAsync(candidate => candidate.Name == normalized);
        if (category is not null)
        {
            if (!string.IsNullOrWhiteSpace(description) && string.IsNullOrWhiteSpace(category.Description))
            {
                category.Description = description.Trim();
            }

            return category;
        }

        category = new ServiceCategory
        {
            Name = normalized,
            Description = description?.Trim() ?? string.Empty
        };
        db.ServiceCategories.Add(category);
        return category;
    }

    private async Task SyncCustomerAccessAsync(AppUser user)
    {
        var existingAccess = await db.CustomerSystemAccesses
            .Where(access => access.AppUserId == user.Id)
            .ToListAsync();
        db.CustomerSystemAccesses.RemoveRange(existingAccess);

        if (user.Role == UserRole.Client && user.CustomerId is not null)
        {
            db.CustomerSystemAccesses.Add(new CustomerSystemAccess
            {
                CustomerId = user.CustomerId.Value,
                AppUserId = user.Id
            });
        }

        await db.SaveChangesAsync();
    }
}
