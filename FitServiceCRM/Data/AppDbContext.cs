using FitServiceCRM.Models;
using Microsoft.EntityFrameworkCore;

namespace FitServiceCRM.Data;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<ServiceCenter> ServiceCenters => Set<ServiceCenter>();
    public DbSet<AppUser> Users => Set<AppUser>();
    public DbSet<SystemRole> Roles => Set<SystemRole>();
    public DbSet<AccessRight> AccessRights => Set<AccessRight>();
    public DbSet<AppUserRole> UserRoles => Set<AppUserRole>();
    public DbSet<RoleAccessRight> RoleAccessRights => Set<RoleAccessRight>();
    public DbSet<Employee> Employees => Set<Employee>();
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<IndividualCustomer> IndividualCustomers => Set<IndividualCustomer>();
    public DbSet<LegalCustomer> LegalCustomers => Set<LegalCustomer>();
    public DbSet<CustomerRepresentative> CustomerRepresentatives => Set<CustomerRepresentative>();
    public DbSet<CustomerSystemAccess> CustomerSystemAccesses => Set<CustomerSystemAccess>();
    public DbSet<Vehicle> Vehicles => Set<Vehicle>();
    public DbSet<ServiceCategory> ServiceCategories => Set<ServiceCategory>();
    public DbSet<ServiceItem> Services => Set<ServiceItem>();
    public DbSet<ServiceAppointment> Appointments => Set<ServiceAppointment>();
    public DbSet<AppointmentService> AppointmentServices => Set<AppointmentService>();
    public DbSet<AppointmentVehicle> AppointmentVehicles => Set<AppointmentVehicle>();
    public DbSet<WorkOrder> WorkOrders => Set<WorkOrder>();
    public DbSet<WorkOrderVehicle> WorkOrderVehicles => Set<WorkOrderVehicle>();
    public DbSet<OrderService> OrderServices => Set<OrderService>();
    public DbSet<WorkOrderEmployee> WorkOrderEmployees => Set<WorkOrderEmployee>();
    public DbSet<DiagnosticDocument> DiagnosticDocuments => Set<DiagnosticDocument>();
    public DbSet<SupportDocument> SupportDocuments => Set<SupportDocument>();
    public DbSet<WorkItem> WorkItems => Set<WorkItem>();
    public DbSet<DiagnosticConclusion> DiagnosticConclusions => Set<DiagnosticConclusion>();
    public DbSet<RoleApproval> RoleApprovals => Set<RoleApproval>();
    public DbSet<ChangeRequest> ChangeRequests => Set<ChangeRequest>();
    public DbSet<Message> Messages => Set<Message>();
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<WorkOrderStatusHistory> WorkOrderStatusHistory => Set<WorkOrderStatusHistory>();
    public DbSet<Attachment> Attachments => Set<Attachment>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AppUser>()
            .HasIndex(user => user.Login)
            .IsUnique();

        modelBuilder.Entity<SystemRole>()
            .HasIndex(role => role.Code)
            .IsUnique();

        modelBuilder.Entity<AccessRight>()
            .HasIndex(right => right.Code)
            .IsUnique();

        modelBuilder.Entity<ServiceCenter>()
            .HasIndex(center => center.Name);

        modelBuilder.Entity<AppUserRole>()
            .HasIndex(link => new { link.AppUserId, link.SystemRoleId })
            .IsUnique();

        modelBuilder.Entity<RoleAccessRight>()
            .HasIndex(link => new { link.SystemRoleId, link.AccessRightId })
            .IsUnique();

        modelBuilder.Entity<IndividualCustomer>()
            .HasKey(profile => profile.CustomerId);

        modelBuilder.Entity<LegalCustomer>()
            .HasKey(profile => profile.CustomerId);

        modelBuilder.Entity<OrderService>()
            .HasKey(orderService => new { orderService.WorkOrderId, orderService.ServiceItemId });

        modelBuilder.Entity<WorkOrderEmployee>()
            .HasKey(workOrderEmployee => new { workOrderEmployee.WorkOrderId, workOrderEmployee.EmployeeId });

        modelBuilder.Entity<WorkOrderVehicle>()
            .HasKey(link => new { link.WorkOrderId, link.VehicleId });

        modelBuilder.Entity<AppointmentService>()
            .HasIndex(link => new { link.ServiceAppointmentId, link.ServiceItemId })
            .IsUnique();

        modelBuilder.Entity<AppointmentVehicle>()
            .HasKey(link => new { link.ServiceAppointmentId, link.VehicleId });

        ConfigureEnumStorage(modelBuilder);
        ConfigureRelationships(modelBuilder);
        ConfigureMoney(modelBuilder);
    }

    private static void ConfigureRelationships(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AppUser>()
            .HasOne(user => user.Employee)
            .WithMany(employee => employee.Users)
            .HasForeignKey(user => user.EmployeeId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<AppUser>()
            .HasOne(user => user.Customer)
            .WithMany(customer => customer.Users)
            .HasForeignKey(user => user.CustomerId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<Employee>()
            .HasOne(employee => employee.User)
            .WithOne()
            .HasForeignKey<Employee>(employee => employee.UserId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<IndividualCustomer>()
            .HasOne(profile => profile.Customer)
            .WithOne(customer => customer.IndividualProfile)
            .HasForeignKey<IndividualCustomer>(profile => profile.CustomerId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<LegalCustomer>()
            .HasOne(profile => profile.Customer)
            .WithOne(customer => customer.LegalProfile)
            .HasForeignKey<LegalCustomer>(profile => profile.CustomerId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<CustomerSystemAccess>()
            .HasOne(access => access.Customer)
            .WithMany(customer => customer.SystemAccesses)
            .HasForeignKey(access => access.CustomerId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<CustomerSystemAccess>()
            .HasOne(access => access.AppUser)
            .WithMany(user => user.CustomerAccesses)
            .HasForeignKey(access => access.AppUserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<CustomerSystemAccess>()
            .HasOne(access => access.CustomerRepresentative)
            .WithMany(representative => representative.SystemAccesses)
            .HasForeignKey(access => access.CustomerRepresentativeId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<ServiceItem>()
            .HasOne(service => service.ServiceCategory)
            .WithMany(category => category.Services)
            .HasForeignKey(service => service.ServiceCategoryId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<ServiceAppointment>()
            .HasOne(appointment => appointment.Customer)
            .WithMany(customer => customer.Appointments)
            .HasForeignKey(appointment => appointment.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<ServiceAppointment>()
            .HasOne(appointment => appointment.Vehicle)
            .WithMany(vehicle => vehicle.Appointments)
            .HasForeignKey(appointment => appointment.VehicleId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<AppointmentVehicle>()
            .HasOne(link => link.ServiceAppointment)
            .WithMany(appointment => appointment.Vehicles)
            .HasForeignKey(link => link.ServiceAppointmentId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<AppointmentVehicle>()
            .HasOne(link => link.Vehicle)
            .WithMany(vehicle => vehicle.AppointmentLinks)
            .HasForeignKey(link => link.VehicleId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<ServiceAppointment>()
            .HasOne(appointment => appointment.ServiceCenter)
            .WithMany(center => center.Appointments)
            .HasForeignKey(appointment => appointment.ServiceCenterId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<ServiceAppointment>()
            .HasOne(appointment => appointment.ReceiverEmployee)
            .WithMany()
            .HasForeignKey(appointment => appointment.ReceiverEmployeeId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<WorkOrder>()
            .HasOne(order => order.ServiceAppointment)
            .WithOne(appointment => appointment.WorkOrder)
            .HasForeignKey<WorkOrder>(order => order.ServiceAppointmentId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<WorkOrder>()
            .HasOne(order => order.Customer)
            .WithMany(customer => customer.WorkOrders)
            .HasForeignKey(order => order.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<WorkOrder>()
            .HasOne(order => order.Vehicle)
            .WithMany(vehicle => vehicle.WorkOrders)
            .HasForeignKey(order => order.VehicleId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<WorkOrderVehicle>()
            .HasOne(link => link.WorkOrder)
            .WithMany(order => order.VehiclesInOrder)
            .HasForeignKey(link => link.WorkOrderId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<WorkOrderVehicle>()
            .HasOne(link => link.Vehicle)
            .WithMany(vehicle => vehicle.OrderLinks)
            .HasForeignKey(link => link.VehicleId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<WorkOrder>()
            .HasOne(order => order.ServiceCenter)
            .WithMany(center => center.WorkOrders)
            .HasForeignKey(order => order.ServiceCenterId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<WorkOrder>()
            .HasOne(order => order.ReceiverEmployee)
            .WithMany(employee => employee.ReceivedOrders)
            .HasForeignKey(order => order.ReceiverEmployeeId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<WorkOrder>()
            .HasOne(order => order.ServiceManagerEmployee)
            .WithMany(employee => employee.ManagedOrders)
            .HasForeignKey(order => order.ServiceManagerEmployeeId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<WorkItem>()
            .HasOne(item => item.Vehicle)
            .WithMany(vehicle => vehicle.WorkItems)
            .HasForeignKey(item => item.VehicleId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<WorkItem>()
            .HasOne(item => item.ExecutorEmployee)
            .WithMany(employee => employee.ExecutedWorks)
            .HasForeignKey(item => item.ExecutorEmployeeId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<DiagnosticDocument>()
            .HasOne(document => document.Vehicle)
            .WithMany()
            .HasForeignKey(document => document.VehicleId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<DiagnosticConclusion>()
            .HasOne(conclusion => conclusion.WorkOrder)
            .WithMany(order => order.DiagnosticConclusions)
            .HasForeignKey(conclusion => conclusion.WorkOrderId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<DiagnosticConclusion>()
            .HasOne(conclusion => conclusion.Customer)
            .WithMany()
            .HasForeignKey(conclusion => conclusion.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<DiagnosticConclusion>()
            .HasOne(conclusion => conclusion.Vehicle)
            .WithMany(vehicle => vehicle.DiagnosticConclusions)
            .HasForeignKey(conclusion => conclusion.VehicleId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<DiagnosticConclusion>()
            .HasOne(conclusion => conclusion.ReceiverEmployee)
            .WithMany()
            .HasForeignKey(conclusion => conclusion.ReceiverEmployeeId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<RoleApproval>()
            .HasOne(approval => approval.ApproverUser)
            .WithMany(user => user.Approvals)
            .HasForeignKey(approval => approval.ApproverUserId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<ChangeRequest>()
            .HasOne(request => request.AuthorUser)
            .WithMany()
            .HasForeignKey(request => request.AuthorUserId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<ChangeRequest>()
            .HasOne(request => request.RecipientUser)
            .WithMany()
            .HasForeignKey(request => request.RecipientUserId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Message>()
            .HasOne(message => message.SenderUser)
            .WithMany(user => user.SentMessages)
            .HasForeignKey(message => message.SenderUserId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Message>()
            .HasOne(message => message.RecipientUser)
            .WithMany(user => user.ReceivedMessages)
            .HasForeignKey(message => message.RecipientUserId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Notification>()
            .HasOne(notification => notification.AppUser)
            .WithMany(user => user.Notifications)
            .HasForeignKey(notification => notification.AppUserId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<Notification>()
            .HasOne(notification => notification.Customer)
            .WithMany(customer => customer.Notifications)
            .HasForeignKey(notification => notification.CustomerId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Notification>()
            .HasOne(notification => notification.DiagnosticConclusion)
            .WithMany()
            .HasForeignKey(notification => notification.DiagnosticConclusionId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<Notification>()
            .HasOne(notification => notification.Vehicle)
            .WithMany()
            .HasForeignKey(notification => notification.VehicleId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<WorkOrderStatusHistory>()
            .HasOne(history => history.ChangedByUser)
            .WithMany()
            .HasForeignKey(history => history.ChangedByUserId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<Attachment>()
            .HasOne(attachment => attachment.UploadedByUser)
            .WithMany()
            .HasForeignKey(attachment => attachment.UploadedByUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }

    private static void ConfigureEnumStorage(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AppUser>().Property(user => user.Role).HasConversion<string>().HasMaxLength(50);
        modelBuilder.Entity<Customer>().Property(customer => customer.Type).HasConversion<string>().HasMaxLength(30);
        modelBuilder.Entity<Customer>().Property(customer => customer.Status).HasConversion<string>().HasMaxLength(30);
        modelBuilder.Entity<ServiceAppointment>().Property(appointment => appointment.Status).HasConversion<string>().HasMaxLength(30);
        modelBuilder.Entity<WorkOrder>().Property(order => order.Status).HasConversion<string>().HasMaxLength(30);
        modelBuilder.Entity<WorkItem>().Property(item => item.Status).HasConversion<string>().HasMaxLength(30);
        modelBuilder.Entity<DiagnosticConclusion>().Property(conclusion => conclusion.Status).HasConversion<string>().HasMaxLength(30);
        modelBuilder.Entity<RoleApproval>().Property(approval => approval.ApproverRole).HasConversion<string>().HasMaxLength(50);
        modelBuilder.Entity<RoleApproval>().Property(approval => approval.Decision).HasConversion<string>().HasMaxLength(30);
        modelBuilder.Entity<ChangeRequest>().Property(request => request.Status).HasConversion<string>().HasMaxLength(30);
        modelBuilder.Entity<Notification>().Property(notification => notification.Kind).HasConversion<string>().HasMaxLength(50);
        modelBuilder.Entity<Notification>().Property(notification => notification.Channel).HasConversion<string>().HasMaxLength(30);
        modelBuilder.Entity<Notification>().Property(notification => notification.DeliveryStatus).HasConversion<string>().HasMaxLength(30);
        modelBuilder.Entity<WorkOrderStatusHistory>().Property(history => history.PreviousStatus).HasConversion<string>().HasMaxLength(30);
        modelBuilder.Entity<WorkOrderStatusHistory>().Property(history => history.NewStatus).HasConversion<string>().HasMaxLength(30);
        modelBuilder.Entity<AuditLog>().Property(log => log.Action).HasConversion<string>().HasMaxLength(50);
        modelBuilder.Entity<SupportDocument>().Property(document => document.Kind).HasConversion<string>().HasMaxLength(50);
    }

    private static void ConfigureMoney(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ServiceItem>().Property(service => service.Price).HasPrecision(12, 2);
        modelBuilder.Entity<ServiceItem>().Property(service => service.BasePrice).HasPrecision(12, 2);
        modelBuilder.Entity<WorkItem>().Property(work => work.Price).HasPrecision(12, 2);
        modelBuilder.Entity<WorkItem>().Property(work => work.UnitPrice).HasPrecision(12, 2);
        modelBuilder.Entity<WorkItem>().Property(work => work.Discount).HasPrecision(5, 2);
        modelBuilder.Entity<WorkItem>().Property(work => work.TotalAmount).HasPrecision(12, 2);
        modelBuilder.Entity<WorkItem>().Property(work => work.Quantity).HasPrecision(8, 2);
        modelBuilder.Entity<WorkOrder>().Property(order => order.TotalCost).HasPrecision(12, 2);
    }
}
