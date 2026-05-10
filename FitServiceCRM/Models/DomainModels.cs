using System.ComponentModel.DataAnnotations;

namespace FitServiceCRM.Models;

public sealed class ServiceCenter
{
    public int Id { get; set; }

    [MaxLength(150)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(300)]
    public string Address { get; set; } = string.Empty;

    [MaxLength(20)]
    public string Phone { get; set; } = string.Empty;

    [MaxLength(254)]
    public string Email { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;

    public List<ServiceAppointment> Appointments { get; set; } = [];
    public List<WorkOrder> WorkOrders { get; set; } = [];
}

public sealed class AppUser
{
    public int Id { get; set; }

    [MaxLength(100)]
    public string Login { get; set; } = string.Empty;

    [MaxLength(255)]
    public string PasswordHash { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;

    public UserRole Role { get; set; }

    public int? EmployeeId { get; set; }
    public Employee? Employee { get; set; }

    public int? CustomerId { get; set; }
    public Customer? Customer { get; set; }

    public List<AppUserRole> RoleAssignments { get; set; } = [];
    public List<CustomerSystemAccess> CustomerAccesses { get; set; } = [];
    public List<RoleApproval> Approvals { get; set; } = [];
    public List<Message> SentMessages { get; set; } = [];
    public List<Message> ReceivedMessages { get; set; } = [];
    public List<Notification> Notifications { get; set; } = [];
}

public sealed class SystemRole
{
    public int Id { get; set; }

    [MaxLength(50)]
    public string Code { get; set; } = string.Empty;

    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(500)]
    public string Description { get; set; } = string.Empty;

    public List<AppUserRole> Users { get; set; } = [];
    public List<RoleAccessRight> AccessRights { get; set; } = [];
}

public sealed class AccessRight
{
    public int Id { get; set; }

    [MaxLength(100)]
    public string Code { get; set; } = string.Empty;

    [MaxLength(100)]
    public string ModuleName { get; set; } = string.Empty;

    [MaxLength(500)]
    public string Description { get; set; } = string.Empty;

    public List<RoleAccessRight> Roles { get; set; } = [];
}

public sealed class AppUserRole
{
    public int Id { get; set; }

    public int AppUserId { get; set; }
    public AppUser? AppUser { get; set; }

    public int SystemRoleId { get; set; }
    public SystemRole? SystemRole { get; set; }
}

public sealed class RoleAccessRight
{
    public int Id { get; set; }

    public int SystemRoleId { get; set; }
    public SystemRole? SystemRole { get; set; }

    public int AccessRightId { get; set; }
    public AccessRight? AccessRight { get; set; }
}

public sealed class Employee
{
    public int Id { get; set; }

    public int? UserId { get; set; }
    public AppUser? User { get; set; }

    [MaxLength(200)]
    public string FullName { get; set; } = string.Empty;

    [MaxLength(100)]
    public string Position { get; set; } = string.Empty;

    [MaxLength(20)]
    public string Phone { get; set; } = string.Empty;

    [MaxLength(254)]
    public string Email { get; set; } = string.Empty;

    public List<AppUser> Users { get; set; } = [];
    public List<WorkOrderEmployee> WorkOrders { get; set; } = [];
    public List<WorkOrder> ReceivedOrders { get; set; } = [];
    public List<WorkOrder> ManagedOrders { get; set; } = [];
    public List<WorkItem> ExecutedWorks { get; set; } = [];
}

public sealed class Customer
{
    public int Id { get; set; }

    public CustomerType Type { get; set; }
    public CustomerStatus Status { get; set; } = CustomerStatus.Active;

    [MaxLength(1000)]
    public string Comment { get; set; } = string.Empty;

    [MaxLength(180)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(32)]
    public string Phone { get; set; } = string.Empty;

    [MaxLength(120)]
    public string Email { get; set; } = string.Empty;

    [MaxLength(300)]
    public string Address { get; set; } = string.Empty;

    [MaxLength(24)]
    public string TaxNumber { get; set; } = string.Empty;

    public IndividualCustomer? IndividualProfile { get; set; }
    public LegalCustomer? LegalProfile { get; set; }
    public List<CustomerRepresentative> Representatives { get; set; } = [];
    public List<CustomerSystemAccess> SystemAccesses { get; set; } = [];
    public List<Vehicle> Vehicles { get; set; } = [];
    public List<ServiceAppointment> Appointments { get; set; } = [];
    public List<WorkOrder> WorkOrders { get; set; } = [];
    public List<AppUser> Users { get; set; } = [];
    public List<Notification> Notifications { get; set; } = [];
}

public sealed class IndividualCustomer
{
    public int CustomerId { get; set; }
    public Customer? Customer { get; set; }

    [MaxLength(100)]
    public string FullName { get; set; } = string.Empty;

    public DateOnly? BirthDate { get; set; }

    [MaxLength(20)]
    public string Phone { get; set; } = string.Empty;

    [MaxLength(254)]
    public string Email { get; set; } = string.Empty;
}

public sealed class LegalCustomer
{
    public int CustomerId { get; set; }
    public Customer? Customer { get; set; }

    [MaxLength(255)]
    public string OrganizationName { get; set; } = string.Empty;

    [MaxLength(12)]
    public string Inn { get; set; } = string.Empty;

    [MaxLength(9)]
    public string Kpp { get; set; } = string.Empty;

    [MaxLength(15)]
    public string Ogrn { get; set; } = string.Empty;

    [MaxLength(300)]
    public string LegalAddress { get; set; } = string.Empty;

    [MaxLength(300)]
    public string ActualAddress { get; set; } = string.Empty;

    [MaxLength(20)]
    public string Phone { get; set; } = string.Empty;

    [MaxLength(254)]
    public string Email { get; set; } = string.Empty;
}

public sealed class CustomerRepresentative
{
    public int Id { get; set; }

    public int CustomerId { get; set; }
    public Customer? Customer { get; set; }

    [MaxLength(255)]
    public string FullName { get; set; } = string.Empty;

    [MaxLength(100)]
    public string Position { get; set; } = string.Empty;

    [MaxLength(20)]
    public string Phone { get; set; } = string.Empty;

    [MaxLength(254)]
    public string Email { get; set; } = string.Empty;

    public List<CustomerSystemAccess> SystemAccesses { get; set; } = [];
}

public sealed class CustomerSystemAccess
{
    public int Id { get; set; }

    public int CustomerId { get; set; }
    public Customer? Customer { get; set; }

    public int AppUserId { get; set; }
    public AppUser? AppUser { get; set; }

    public int? CustomerRepresentativeId { get; set; }
    public CustomerRepresentative? CustomerRepresentative { get; set; }
}

public sealed class Vehicle
{
    public int Id { get; set; }

    public int CustomerId { get; set; }
    public Customer? Customer { get; set; }

    [MaxLength(17)]
    public string Vin { get; set; } = string.Empty;

    [MaxLength(15)]
    public string PlateNumber { get; set; } = string.Empty;

    [MaxLength(100)]
    public string Make { get; set; } = string.Empty;

    [MaxLength(100)]
    public string Model { get; set; } = string.Empty;

    public int Year { get; set; }
    public int Mileage { get; set; }

    [MaxLength(50)]
    public string Color { get; set; } = string.Empty;

    public List<ServiceAppointment> Appointments { get; set; } = [];
    public List<AppointmentVehicle> AppointmentLinks { get; set; } = [];
    public List<WorkOrder> WorkOrders { get; set; } = [];
    public List<WorkOrderVehicle> OrderLinks { get; set; } = [];
    public List<WorkItem> WorkItems { get; set; } = [];
    public List<DiagnosticConclusion> DiagnosticConclusions { get; set; } = [];
}

public sealed class WorkOrderVehicle
{
    public int WorkOrderId { get; set; }
    public WorkOrder? WorkOrder { get; set; }

    public int VehicleId { get; set; }
    public Vehicle? Vehicle { get; set; }
}

public sealed class ServiceCategory
{
    public int Id { get; set; }

    [MaxLength(150)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(500)]
    public string Description { get; set; } = string.Empty;

    public List<ServiceItem> Services { get; set; } = [];
}

public sealed class ServiceItem
{
    public int Id { get; set; }

    public int? ServiceCategoryId { get; set; }
    public ServiceCategory? ServiceCategory { get; set; }

    [MaxLength(150)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string Description { get; set; } = string.Empty;

    public int StandardDurationMinutes { get; set; }
    public decimal BasePrice { get; set; }

    [MaxLength(80)]
    public string Category { get; set; } = string.Empty;

    [MaxLength(80)]
    public string Type { get; set; } = string.Empty;

    public int EstimatedDurationMinutes { get; set; }
    public decimal Price { get; set; }
    public int ApproximatePurchaseIntervalDays { get; set; }

    public List<AppointmentService> AppointmentServices { get; set; } = [];
    public List<OrderService> OrderServices { get; set; } = [];
    public List<WorkItem> WorkItems { get; set; } = [];
}

public sealed class ServiceAppointment
{
    public int Id { get; set; }

    public int CustomerId { get; set; }
    public Customer? Customer { get; set; }

    public int? VehicleId { get; set; }
    public Vehicle? Vehicle { get; set; }

    public int ServiceCenterId { get; set; }
    public ServiceCenter? ServiceCenter { get; set; }

    public int? ReceiverEmployeeId { get; set; }
    public Employee? ReceiverEmployee { get; set; }

    public DateTime PlannedVisitAt { get; set; }
    public AppointmentStatus Status { get; set; }

    [MaxLength(1000)]
    public string CustomerComment { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public List<AppointmentService> Services { get; set; } = [];
    public List<AppointmentVehicle> Vehicles { get; set; } = [];
    public WorkOrder? WorkOrder { get; set; }
}

public sealed class AppointmentVehicle
{
    public int ServiceAppointmentId { get; set; }
    public ServiceAppointment? ServiceAppointment { get; set; }

    public int VehicleId { get; set; }
    public Vehicle? Vehicle { get; set; }
}

public sealed class AppointmentService
{
    public int Id { get; set; }

    public int ServiceAppointmentId { get; set; }
    public ServiceAppointment? ServiceAppointment { get; set; }

    public int ServiceItemId { get; set; }
    public ServiceItem? ServiceItem { get; set; }
}

public sealed class WorkOrder
{
    public int Id { get; set; }

    [MaxLength(50)]
    public string OrderNumber { get; set; } = string.Empty;

    public int? ServiceAppointmentId { get; set; }
    public ServiceAppointment? ServiceAppointment { get; set; }

    public int CustomerId { get; set; }
    public Customer? Customer { get; set; }

    public int VehicleId { get; set; }
    public Vehicle? Vehicle { get; set; }

    public int ServiceCenterId { get; set; }
    public ServiceCenter? ServiceCenter { get; set; }

    public int ReceiverEmployeeId { get; set; }
    public Employee? ReceiverEmployee { get; set; }

    public int? ServiceManagerEmployeeId { get; set; }
    public Employee? ServiceManagerEmployee { get; set; }

    public WorkOrderStatus Status { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime UpdatedAt { get; set; } = DateTime.Now;
    public DateTime PlannedStartAt { get; set; }
    public DateTime? PlannedEndAt { get; set; }
    public DateTime? ActualStartAt { get; set; }
    public DateTime? ActualEndAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public int PlannedDurationMinutes { get; set; }
    public int? ActualDurationMinutes { get; set; }

    public decimal TotalCost { get; set; }

    [MaxLength(1000)]
    public string CustomerComment { get; set; } = string.Empty;

    [MaxLength(2000)]
    public string Comment { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string InternalComment { get; set; } = string.Empty;

    public DateTime? ClientApprovedAt { get; set; }

    public List<WorkOrderVehicle> VehiclesInOrder { get; set; } = [];
    public List<OrderService> OrderServices { get; set; } = [];
    public List<WorkOrderEmployee> Executors { get; set; } = [];
    public List<DiagnosticDocument> DiagnosticDocuments { get; set; } = [];
    public List<SupportDocument> SupportDocuments { get; set; } = [];
    public List<WorkItem> WorkItems { get; set; } = [];
    public List<DiagnosticConclusion> DiagnosticConclusions { get; set; } = [];
    public List<RoleApproval> Approvals { get; set; } = [];
    public List<ChangeRequest> ChangeRequests { get; set; } = [];
    public List<Message> Messages { get; set; } = [];
    public List<Notification> Notifications { get; set; } = [];
    public List<WorkOrderStatusHistory> StatusHistory { get; set; } = [];
    public List<Attachment> Attachments { get; set; } = [];
}

public sealed class OrderService
{
    public int? WorkOrderId { get; set; }
    public WorkOrder? WorkOrder { get; set; }

    public int CustomerId { get; set; }
    public Customer? Customer { get; set; }

    public int ServiceItemId { get; set; }
    public ServiceItem? ServiceItem { get; set; }
}

public sealed class WorkOrderEmployee
{
    public int? WorkOrderId { get; set; }
    public WorkOrder? WorkOrder { get; set; }

    public int CustomerId { get; set; }
    public Customer? Customer { get; set; }

    public int EmployeeId { get; set; }
    public Employee? Employee { get; set; }
}

public sealed class WorkItem
{
    public int Id { get; set; }

    public int WorkOrderId { get; set; }
    public WorkOrder? WorkOrder { get; set; }

    public int VehicleId { get; set; }
    public Vehicle? Vehicle { get; set; }

    public int ServiceItemId { get; set; }
    public ServiceItem? ServiceItem { get; set; }

    [MaxLength(1000)]
    public string Description { get; set; } = string.Empty;

    public int? ExecutorEmployeeId { get; set; }
    public Employee? ExecutorEmployee { get; set; }

    public decimal Quantity { get; set; } = 1;
    public decimal UnitPrice { get; set; }
    public decimal Discount { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal Price { get; set; }
    public WorkItemStatus Status { get; set; } = WorkItemStatus.Planned;
    public DateTime? PlannedStartAt { get; set; }
    public DateTime? PlannedEndAt { get; set; }
    public DateTime? ActualStartAt { get; set; }
    public DateTime? ActualEndAt { get; set; }
}

public sealed class DiagnosticConclusion
{
    public int Id { get; set; }

    public int? WorkOrderId { get; set; }
    public WorkOrder? WorkOrder { get; set; }

    public int CustomerId { get; set; }
    public Customer? Customer { get; set; }

    public int VehicleId { get; set; }
    public Vehicle? Vehicle { get; set; }

    public int ReceiverEmployeeId { get; set; }
    public Employee? ReceiverEmployee { get; set; }

    [MaxLength(5000)]
    public string DiagnosticResult { get; set; } = string.Empty;

    [MaxLength(5000)]
    public string Recommendations { get; set; } = string.Empty;

    public DiagnosticConclusionStatus Status { get; set; } = DiagnosticConclusionStatus.Draft;
    public DateTime CreatedAt { get; set; } = DateTime.Now;
}

public sealed class RoleApproval
{
    public int Id { get; set; }

    public int WorkOrderId { get; set; }
    public WorkOrder? WorkOrder { get; set; }

    public int ApproverUserId { get; set; }
    public AppUser? ApproverUser { get; set; }

    public ApprovalRole ApproverRole { get; set; }
    public ApprovalDecision Decision { get; set; } = ApprovalDecision.Pending;

    [MaxLength(1000)]
    public string DecisionComment { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime? DecidedAt { get; set; }
}

public sealed class ChangeRequest
{
    public int Id { get; set; }

    public int WorkOrderId { get; set; }
    public WorkOrder? WorkOrder { get; set; }

    public int AuthorUserId { get; set; }
    public AppUser? AuthorUser { get; set; }

    public int RecipientUserId { get; set; }
    public AppUser? RecipientUser { get; set; }

    [MaxLength(2000)]
    public string RequiredChanges { get; set; } = string.Empty;

    public ChangeRequestStatus Status { get; set; } = ChangeRequestStatus.Open;
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime? ClosedAt { get; set; }

    public List<Message> Messages { get; set; } = [];
}

public sealed class Message
{
    public int Id { get; set; }

    public int? WorkOrderId { get; set; }
    public WorkOrder? WorkOrder { get; set; }

    public int? ChangeRequestId { get; set; }
    public ChangeRequest? ChangeRequest { get; set; }

    public int SenderUserId { get; set; }
    public AppUser? SenderUser { get; set; }

    public int RecipientUserId { get; set; }
    public AppUser? RecipientUser { get; set; }

    [MaxLength(200)]
    public string Subject { get; set; } = string.Empty;

    [MaxLength(5000)]
    public string Body { get; set; } = string.Empty;

    public DateTime SentAt { get; set; } = DateTime.Now;
    public DateTime? ReadAt { get; set; }

    public List<Attachment> Attachments { get; set; } = [];
}

public sealed class Notification
{
    public int Id { get; set; }

    public int? AppUserId { get; set; }
    public AppUser? AppUser { get; set; }

    public int CustomerId { get; set; }
    public Customer? Customer { get; set; }

    public int? WorkOrderId { get; set; }
    public WorkOrder? WorkOrder { get; set; }

    public int? DiagnosticConclusionId { get; set; }
    public DiagnosticConclusion? DiagnosticConclusion { get; set; }

    public int? VehicleId { get; set; }
    public Vehicle? Vehicle { get; set; }

    public NotificationKind Kind { get; set; } = NotificationKind.Message;
    public NotificationChannel Channel { get; set; } = NotificationChannel.System;
    public NotificationDeliveryStatus DeliveryStatus { get; set; } = NotificationDeliveryStatus.Created;

    [MaxLength(180)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string Message { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime? SentAt { get; set; }
    public DateTime? ReadAt { get; set; }
    public bool IsRead { get; set; }
}

public sealed class WorkOrderStatusHistory
{
    public int Id { get; set; }

    public int WorkOrderId { get; set; }
    public WorkOrder? WorkOrder { get; set; }

    public WorkOrderStatus? PreviousStatus { get; set; }
    public WorkOrderStatus NewStatus { get; set; }

    public int? ChangedByUserId { get; set; }
    public AppUser? ChangedByUser { get; set; }

    [MaxLength(1000)]
    public string Comment { get; set; } = string.Empty;

    public DateTime ChangedAt { get; set; } = DateTime.Now;
}

public sealed class Attachment
{
    public int Id { get; set; }

    public int? WorkOrderId { get; set; }
    public WorkOrder? WorkOrder { get; set; }

    public int? MessageId { get; set; }
    public Message? Message { get; set; }

    [MaxLength(255)]
    public string FileName { get; set; } = string.Empty;

    [MaxLength(100)]
    public string ContentType { get; set; } = string.Empty;

    [MaxLength(500)]
    public string Path { get; set; } = string.Empty;

    public long SizeBytes { get; set; }

    public int UploadedByUserId { get; set; }
    public AppUser? UploadedByUser { get; set; }

    public DateTime UploadedAt { get; set; } = DateTime.Now;
}

public sealed class AuditLog
{
    public int Id { get; set; }

    public int? AppUserId { get; set; }
    public AppUser? AppUser { get; set; }

    [MaxLength(100)]
    public string EntityName { get; set; } = string.Empty;

    public int EntityId { get; set; }
    public AuditAction Action { get; set; }

    [MaxLength(5000)]
    public string OldValue { get; set; } = string.Empty;

    [MaxLength(5000)]
    public string NewValue { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.Now;
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

    public DateTime CreatedAt { get; set; } = DateTime.Now;
}
