using System.ComponentModel.DataAnnotations;

namespace FitServiceCRM.Models;

public sealed class AppUser
{
    public int Id { get; set; }

    [MaxLength(64)]
    public string Login { get; set; } = string.Empty;

    [MaxLength(128)]
    public string PasswordHash { get; set; } = string.Empty;

    public UserRole Role { get; set; }

    public bool IsActive { get; set; } = true;

    public int? EmployeeId { get; set; }
    public Employee? Employee { get; set; }

    public int? CustomerId { get; set; }
    public Customer? Customer { get; set; }
}

public sealed class Employee
{
    public int Id { get; set; }

    [MaxLength(160)]
    public string FullName { get; set; } = string.Empty;

    [MaxLength(32)]
    public string Phone { get; set; } = string.Empty;

    [MaxLength(120)]
    public string Email { get; set; } = string.Empty;

    [MaxLength(120)]
    public string Position { get; set; } = string.Empty;

    public List<AppUser> Users { get; set; } = [];
    public List<WorkOrderEmployee> WorkOrders { get; set; } = [];
}

public sealed class Customer
{
    public int Id { get; set; }

    public CustomerType Type { get; set; }

    [MaxLength(180)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(32)]
    public string Phone { get; set; } = string.Empty;

    [MaxLength(120)]
    public string Email { get; set; } = string.Empty;

    [MaxLength(240)]
    public string Address { get; set; } = string.Empty;

    [MaxLength(24)]
    public string TaxNumber { get; set; } = string.Empty;

    public List<Vehicle> Vehicles { get; set; } = [];
    public List<WorkOrder> WorkOrders { get; set; } = [];
    public List<AppUser> Users { get; set; } = [];
    public List<Notification> Notifications { get; set; } = [];
}

public sealed class Vehicle
{
    public int Id { get; set; }

    public int CustomerId { get; set; }
    public Customer? Customer { get; set; }

    [MaxLength(80)]
    public string Make { get; set; } = string.Empty;

    [MaxLength(80)]
    public string Model { get; set; } = string.Empty;

    [MaxLength(24)]
    public string PlateNumber { get; set; } = string.Empty;

    public int Year { get; set; }
    public int Mileage { get; set; }

    public List<WorkOrder> WorkOrders { get; set; } = [];
    public List<WorkItem> WorkItems { get; set; } = [];
}

public sealed class ServiceItem
{
    public int Id { get; set; }

    [MaxLength(160)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(80)]
    public string Category { get; set; } = string.Empty;

    [MaxLength(80)]
    public string Type { get; set; } = string.Empty;

    public int EstimatedDurationMinutes { get; set; }
    public decimal Price { get; set; }
    public int ApproximatePurchaseIntervalDays { get; set; }

    public List<OrderService> OrderServices { get; set; } = [];
    public List<WorkItem> WorkItems { get; set; } = [];
}

public sealed class WorkOrder
{
    public int Id { get; set; }

    public int CustomerId { get; set; }
    public Customer? Customer { get; set; }

    public int VehicleId { get; set; }
    public Vehicle? Vehicle { get; set; }

    public int ReceiverEmployeeId { get; set; }
    public Employee? ReceiverEmployee { get; set; }

    public WorkOrderStatus Status { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime PlannedStartAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public int PlannedDurationMinutes { get; set; }
    public int? ActualDurationMinutes { get; set; }

    public decimal TotalCost { get; set; }

    [MaxLength(1000)]
    public string CustomerComment { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string InternalComment { get; set; } = string.Empty;

    public DateTime? ClientApprovedAt { get; set; }

    public List<OrderService> OrderServices { get; set; } = [];
    public List<WorkOrderEmployee> Executors { get; set; } = [];
    public List<DiagnosticDocument> DiagnosticDocuments { get; set; } = [];
    public List<SupportDocument> SupportDocuments { get; set; } = [];
    public List<WorkItem> WorkItems { get; set; } = [];
}

public sealed class OrderService
{
    public int WorkOrderId { get; set; }
    public WorkOrder? WorkOrder { get; set; }

    public int ServiceItemId { get; set; }
    public ServiceItem? ServiceItem { get; set; }
}

public sealed class WorkOrderEmployee
{
    public int WorkOrderId { get; set; }
    public WorkOrder? WorkOrder { get; set; }

    public int EmployeeId { get; set; }
    public Employee? Employee { get; set; }
}

public sealed class DiagnosticDocument
{
    public int Id { get; set; }

    public int WorkOrderId { get; set; }
    public WorkOrder? WorkOrder { get; set; }

    public int EmployeeId { get; set; }
    public Employee? Employee { get; set; }

    public int CustomerId { get; set; }
    public Customer? Customer { get; set; }

    public int VehicleId { get; set; }
    public Vehicle? Vehicle { get; set; }

    public DateTime PerformedAt { get; set; }

    [MaxLength(1600)]
    public string Summary { get; set; } = string.Empty;

    [MaxLength(240)]
    public string FileName { get; set; } = string.Empty;
}

public sealed class SupportDocument
{
    public int Id { get; set; }

    public int WorkOrderId { get; set; }
    public WorkOrder? WorkOrder { get; set; }

    public DocumentKind Kind { get; set; }

    [MaxLength(240)]
    public string FileName { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public sealed class WorkItem
{
    public int Id { get; set; }

    public int WorkOrderId { get; set; }
    public WorkOrder? WorkOrder { get; set; }

    public int ServiceItemId { get; set; }
    public ServiceItem? ServiceItem { get; set; }

    public int VehicleId { get; set; }
    public Vehicle? Vehicle { get; set; }

    [MaxLength(1000)]
    public string Description { get; set; } = string.Empty;

    public decimal Price { get; set; }
}

public sealed class Notification
{
    public int Id { get; set; }

    public int CustomerId { get; set; }
    public Customer? Customer { get; set; }

    public int? WorkOrderId { get; set; }
    public WorkOrder? WorkOrder { get; set; }

    [MaxLength(180)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(800)]
    public string Message { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public bool IsRead { get; set; }
}
