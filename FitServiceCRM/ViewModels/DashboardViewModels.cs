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
    public List<ServiceCenter> ServiceCenters { get; set; } = [];
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
    public int ServiceCentersTotal { get; set; }
}

public sealed class AdminUsersSectionViewModel
{
    public List<AppUser> Users { get; set; } = [];
    public AppUser? SelectedUser { get; set; }
    public List<SystemRole> Roles { get; set; } = [];
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
    public List<AppUser> Users { get; set; } = [];
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
    public CustomerRepresentative? SelectedRepresentative { get; set; }
    public string? Query { get; set; }
    public string Sort { get; set; } = "name";
    public CustomerType? Type { get; set; }
    public CustomerStatus? Status { get; set; }
    public bool IsCreate { get; set; }
    public bool IsEdit { get; set; }
    public bool IsVehicleCreate { get; set; }
    public bool IsVehicleEdit { get; set; }
    public bool IsRepresentativeCreate { get; set; }
    public bool IsRepresentativeEdit { get; set; }
    public bool ShowCard => IsCreate || SelectedCustomer is not null;
    public bool ShowVehicleCard => IsEdit && SelectedCustomer is not null && (IsVehicleCreate || SelectedVehicle is not null);
    public bool ShowRepresentativeCard => IsEdit && SelectedCustomer is not null && (IsRepresentativeCreate || SelectedRepresentative is not null);
}

public sealed class AdminServicesSectionViewModel
{
    public List<ServiceItem> Services { get; set; } = [];
    public ServiceItem? SelectedService { get; set; }
    public List<ServiceCategory> ServiceCategories { get; set; } = [];
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
    public WorkItem? SelectedWorkItem { get; set; }
    public List<ServiceCenter> ServiceCenters { get; set; } = [];
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
    public bool IsWorkItemCreate { get; set; }
    public bool IsWorkItemEdit { get; set; }
    public bool ShowCard => IsCreate || SelectedOrder is not null;
    public bool ShowWorkItemCard => IsEdit && SelectedOrder is not null && (IsWorkItemCreate || SelectedWorkItem is not null);
}

public sealed class WorkOrderHistoryEntryViewModel
{
    public DateTime At { get; set; }
    public string EventType { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? Result { get; set; }
}

public sealed class ManagerDashboardViewModel
{
    public List<WorkOrder> RecentOrders { get; set; } = [];
    public List<Customer> Customers { get; set; } = [];
    public List<Employee> Employees { get; set; } = [];
    public List<ServiceItem> Services { get; set; } = [];
}

public sealed class ManagerOrdersSectionViewModel
{
    public List<WorkOrder> Orders { get; set; } = [];
    public WorkOrder? SelectedOrder { get; set; }
    public CustomerRepresentative? SelectedRepresentative { get; set; }
    public Vehicle? SelectedVehicle { get; set; }
    public ServiceItem? SelectedService { get; set; }
    public Employee? SelectedEmployee { get; set; }
    public DiagnosticConclusion? SelectedDiagnostic { get; set; }
    public List<WorkOrderHistoryEntryViewModel> History { get; set; } = [];
    public string? Query { get; set; }
    public string Sort { get; set; } = "date_desc";
    public WorkOrderStatus? Status { get; set; }
    public DateTime? From { get; set; }
    public DateTime? To { get; set; }
    public bool ShowHistory { get; set; }
    public bool ShowNestedCard => SelectedRepresentative is not null || SelectedVehicle is not null || SelectedService is not null || SelectedEmployee is not null || SelectedDiagnostic is not null;
}

public sealed class ManagerCustomersSectionViewModel
{
    public List<Customer> Customers { get; set; } = [];
    public Customer? SelectedCustomer { get; set; }
    public CustomerRepresentative? SelectedRepresentative { get; set; }
    public Vehicle? SelectedVehicle { get; set; }
    public string? Query { get; set; }
    public string Sort { get; set; } = "name";
    public bool ShowNestedCard => SelectedRepresentative is not null || SelectedVehicle is not null;
}

public sealed class ManagerEmployeesSectionViewModel
{
    public List<Employee> Employees { get; set; } = [];
    public Employee? SelectedEmployee { get; set; }
    public string? Query { get; set; }
    public string Sort { get; set; } = "name";
}

public sealed class ManagerServicesSectionViewModel
{
    public List<ServiceItem> Services { get; set; } = [];
    public ServiceItem? SelectedService { get; set; }
    public string? Query { get; set; }
    public string Sort { get; set; } = "name";
    public string? Category { get; set; }
    public List<string> Categories { get; set; } = [];
}

public sealed class MasterDashboardViewModel
{
    public List<WorkOrder> RecentOrders { get; set; } = [];
    public List<DiagnosticConclusion> Diagnostics { get; set; } = [];
    public List<Customer> Customers { get; set; } = [];
    public List<ServiceItem> Services { get; set; } = [];
}

public sealed class MasterOrdersSectionViewModel
{
    public int CurrentEmployeeId { get; set; }
    public List<WorkOrder> Orders { get; set; } = [];
    public WorkOrder? SelectedOrder { get; set; }
    public WorkItem? SelectedWorkItem { get; set; }
    public DiagnosticConclusion? SelectedDiagnostic { get; set; }
    public List<Customer> Customers { get; set; } = [];
    public List<Vehicle> Vehicles { get; set; } = [];
    public List<Employee> Employees { get; set; } = [];
    public List<ServiceItem> Services { get; set; } = [];
    public List<ServiceCenter> ServiceCenters { get; set; } = [];
    public List<DiagnosticConclusion> Diagnostics { get; set; } = [];
    public List<WorkOrderHistoryEntryViewModel> History { get; set; } = [];
    public string? Query { get; set; }
    public string Sort { get; set; } = "date_desc";
    public WorkOrderStatus? Status { get; set; }
    public DateTime? From { get; set; }
    public DateTime? To { get; set; }
    public bool IsCreate { get; set; }
    public bool IsEdit { get; set; }
    public bool IsWorkItemCreate { get; set; }
    public bool IsWorkItemEdit { get; set; }
    public bool ShowHistory { get; set; }
    public int? SourceDiagnosticId { get; set; }
    public bool ShowCard => IsCreate || SelectedOrder is not null;
    public bool ShowWorkItemCard => IsEdit && SelectedOrder is not null && (IsWorkItemCreate || SelectedWorkItem is not null);
}

public sealed class MasterDiagnosticsSectionViewModel
{
    public int CurrentEmployeeId { get; set; }
    public List<DiagnosticConclusion> Diagnostics { get; set; } = [];
    public DiagnosticConclusion? SelectedDiagnostic { get; set; }
    public WorkOrder? SelectedOrder { get; set; }
    public List<Customer> Customers { get; set; } = [];
    public List<Vehicle> Vehicles { get; set; } = [];
    public List<Employee> Employees { get; set; } = [];
    public List<WorkOrder> Orders { get; set; } = [];
    public string? Query { get; set; }
    public string Sort { get; set; } = "date_desc";
    public DiagnosticConclusionStatus? Status { get; set; }
    public DateTime? From { get; set; }
    public DateTime? To { get; set; }
    public bool IsCreate { get; set; }
    public bool IsEdit { get; set; }
    public bool ShowCard => IsCreate || SelectedDiagnostic is not null;
}

public sealed class ClientDashboardViewModel
{
    public Customer Customer { get; set; } = new();
    public List<Notification> Notifications { get; set; } = [];
    public List<Vehicle> Vehicles { get; set; } = [];
    public List<WorkOrder> Orders { get; set; } = [];
    public List<DiagnosticConclusion> Diagnostics { get; set; } = [];
    public Employee? ServiceManager { get; set; }
    public List<ClientContactCardViewModel> Contacts { get; set; } = [];
}

public sealed class ClientContactCardViewModel
{
    public Employee Employee { get; set; } = new();
    public Vehicle? Vehicle { get; set; }
    public WorkOrder? WorkOrder { get; set; }
}

public sealed class ClientAppointmentsSectionViewModel
{
    public Customer Customer { get; set; } = new();
    public List<Vehicle> Vehicles { get; set; } = [];
    public List<ServiceItem> Services { get; set; } = [];
    public List<ServiceCenter> ServiceCenters { get; set; } = [];
    public List<ServiceAppointment> Appointments { get; set; } = [];
    public string? Query { get; set; }
    public string Sort { get; set; } = "date_desc";
}

public sealed class ClientProfileSectionViewModel
{
    public Customer Customer { get; set; } = new();
    public CustomerRepresentative? SelectedRepresentative { get; set; }
    public bool IsRepresentativeCreate { get; set; }
    public bool IsRepresentativeEdit { get; set; }
    public bool ShowRepresentativeCard => Customer.Type == CustomerType.Company && (IsRepresentativeCreate || SelectedRepresentative is not null);
}

public sealed class ClientVehiclesSectionViewModel
{
    public Customer Customer { get; set; } = new();
    public List<Vehicle> Vehicles { get; set; } = [];
    public Vehicle? SelectedVehicle { get; set; }
    public WorkOrder? SelectedOrder { get; set; }
    public DiagnosticConclusion? SelectedDiagnostic { get; set; }
    public List<WorkOrder> VehicleOrders { get; set; } = [];
    public List<DiagnosticConclusion> VehicleDiagnostics { get; set; } = [];
    public string? Query { get; set; }
    public bool IsCreate { get; set; }
    public bool IsEdit { get; set; }
    public bool ShowHistory { get; set; }
    public bool ShowNestedCard => SelectedOrder is not null || SelectedDiagnostic is not null;
    public bool ShowCard => IsCreate || SelectedVehicle is not null;
}

public sealed class ClientOrdersSectionViewModel
{
    public Customer Customer { get; set; } = new();
    public List<WorkOrder> Orders { get; set; } = [];
    public WorkOrder? SelectedOrder { get; set; }
    public Vehicle? SelectedVehicle { get; set; }
    public DiagnosticConclusion? SelectedDiagnostic { get; set; }
    public List<WorkOrderHistoryEntryViewModel> History { get; set; } = [];
    public string? Query { get; set; }
    public string Sort { get; set; } = "date_desc";
    public WorkOrderStatus? Status { get; set; }
    public DateTime? From { get; set; }
    public bool ShowHistory { get; set; }
    public bool ShowNestedCard => SelectedVehicle is not null || SelectedDiagnostic is not null;
}

public sealed class ClientDiagnosticsSectionViewModel
{
    public Customer Customer { get; set; } = new();
    public List<DiagnosticConclusion> Diagnostics { get; set; } = [];
    public DiagnosticConclusion? SelectedDiagnostic { get; set; }
    public string? Query { get; set; }
    public string Sort { get; set; } = "date_desc";
    public DiagnosticConclusionStatus? Status { get; set; }
    public DateTime? From { get; set; }
}
