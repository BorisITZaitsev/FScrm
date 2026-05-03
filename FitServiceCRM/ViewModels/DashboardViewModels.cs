using FitServiceCRM.Models;
using FitServiceCRM.Services;

namespace FitServiceCRM.ViewModels;

public sealed class LoginViewModel
{
    public string Login { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string? ReturnUrl { get; set; }
    public string? Error { get; set; }
}

public sealed class AdminDashboardViewModel
{
    public List<AppUser> Users { get; set; } = [];
    public List<Employee> Employees { get; set; } = [];
    public List<Customer> Customers { get; set; } = [];
    public List<ServiceItem> Services { get; set; } = [];
    public List<WorkOrder> Orders { get; set; } = [];
    public List<SupportDocument> SupportDocuments { get; set; } = [];
    public List<DiagnosticDocument> DiagnosticDocuments { get; set; } = [];
    public int UsersTotal { get; set; }
    public int EmployeesTotal { get; set; }
    public int CustomersTotal { get; set; }
    public int ServicesTotal { get; set; }
    public int OrdersTotal { get; set; }
}

public sealed class AdminUsersSectionViewModel
{
    public List<AppUser> Users { get; set; } = [];
    public AppUser? SelectedUser { get; set; }
    public List<Employee> Employees { get; set; } = [];
    public List<Customer> Customers { get; set; } = [];
    public string? Query { get; set; }
    public string Sort { get; set; } = "login";
    public UserRole? Role { get; set; }
    public bool? IsActive { get; set; }
    public bool IsCreate { get; set; }
    public bool IsEdit { get; set; }
    public bool ShowCard => IsCreate || SelectedUser is not null;
}

public sealed class AdminEmployeesSectionViewModel
{
    public List<Employee> Employees { get; set; } = [];
    public Employee? SelectedEmployee { get; set; }
    public string? Query { get; set; }
    public string Sort { get; set; } = "name";
    public string? Position { get; set; }
    public List<string> Positions { get; set; } = [];
    public bool IsCreate { get; set; }
    public bool IsEdit { get; set; }
    public bool ShowCard => IsCreate || SelectedEmployee is not null;
}

public sealed class AdminCustomersSectionViewModel
{
    public List<Customer> Customers { get; set; } = [];
    public Customer? SelectedCustomer { get; set; }
    public Vehicle? SelectedVehicle { get; set; }
    public string? Query { get; set; }
    public string Sort { get; set; } = "name";
    public CustomerType? Type { get; set; }
    public bool IsCreate { get; set; }
    public bool IsEdit { get; set; }
    public bool IsVehicleCreate { get; set; }
    public bool IsVehicleEdit { get; set; }
    public bool ShowCard => IsCreate || SelectedCustomer is not null;
    public bool ShowVehicleCard => IsEdit && SelectedCustomer is not null && (IsVehicleCreate || SelectedVehicle is not null);
}

public sealed class AdminServicesSectionViewModel
{
    public List<ServiceItem> Services { get; set; } = [];
    public ServiceItem? SelectedService { get; set; }
    public string? Query { get; set; }
    public string Sort { get; set; } = "name";
    public string? Category { get; set; }
    public string? Type { get; set; }
    public List<string> Categories { get; set; } = [];
    public List<string> Types { get; set; } = [];
    public bool IsCreate { get; set; }
    public bool IsEdit { get; set; }
    public bool ShowCard => IsCreate || SelectedService is not null;
}

public sealed class AdminOrdersSectionViewModel
{
    public List<WorkOrder> Orders { get; set; } = [];
    public WorkOrder? SelectedOrder { get; set; }
    public List<Customer> Customers { get; set; } = [];
    public List<Vehicle> Vehicles { get; set; } = [];
    public List<Employee> Employees { get; set; } = [];
    public List<ServiceItem> Services { get; set; } = [];
    public string? Query { get; set; }
    public string Sort { get; set; } = "date_desc";
    public WorkOrderStatus? Status { get; set; }
    public int? ServiceId { get; set; }
    public bool IsCreate { get; set; }
    public bool IsEdit { get; set; }
    public bool ShowCard => IsCreate || SelectedOrder is not null;
}

public sealed class MasterDashboardViewModel
{
    public int CurrentEmployeeId { get; set; }
    public List<WorkOrder> Orders { get; set; } = [];
    public List<Customer> Customers { get; set; } = [];
    public List<Vehicle> Vehicles { get; set; } = [];
    public List<ServiceItem> Services { get; set; } = [];
    public List<Employee> Employees { get; set; } = [];
}

public sealed class ManagerDashboardViewModel
{
    public List<WorkOrder> RecentOrders { get; set; } = [];
    public List<Employee> Employees { get; set; } = [];
    public List<Customer> Customers { get; set; } = [];
    public List<ServiceItem> Services { get; set; } = [];
    public AnalyticsResult Analytics { get; set; } = new();
    public int? ServiceId { get; set; }
    public int? EmployeeId { get; set; }
    public DateTime? From { get; set; }
    public DateTime? To { get; set; }
}

public sealed class ClientDashboardViewModel
{
    public Customer Customer { get; set; } = new();
    public List<Vehicle> Vehicles { get; set; } = [];
    public List<ServiceItem> Services { get; set; } = [];
    public List<WorkOrder> Orders { get; set; } = [];
    public List<Notification> Notifications { get; set; } = [];
}
