using FitServiceCRM.Models;
using Microsoft.EntityFrameworkCore;

namespace FitServiceCRM.Data;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<AppUser> Users => Set<AppUser>();
    public DbSet<Employee> Employees => Set<Employee>();
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Vehicle> Vehicles => Set<Vehicle>();
    public DbSet<ServiceItem> Services => Set<ServiceItem>();
    public DbSet<WorkOrder> WorkOrders => Set<WorkOrder>();
    public DbSet<OrderService> OrderServices => Set<OrderService>();
    public DbSet<WorkOrderEmployee> WorkOrderEmployees => Set<WorkOrderEmployee>();
    public DbSet<DiagnosticDocument> DiagnosticDocuments => Set<DiagnosticDocument>();
    public DbSet<SupportDocument> SupportDocuments => Set<SupportDocument>();
    public DbSet<WorkItem> WorkItems => Set<WorkItem>();
    public DbSet<Notification> Notifications => Set<Notification>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AppUser>()
            .HasIndex(user => user.Login)
            .IsUnique();

        modelBuilder.Entity<OrderService>()
            .HasKey(orderService => new { orderService.WorkOrderId, orderService.ServiceItemId });

        modelBuilder.Entity<WorkOrderEmployee>()
            .HasKey(workOrderEmployee => new { workOrderEmployee.WorkOrderId, workOrderEmployee.EmployeeId });

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

        modelBuilder.Entity<WorkOrder>()
            .HasOne(order => order.ReceiverEmployee)
            .WithMany()
            .HasForeignKey(order => order.ReceiverEmployeeId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<WorkItem>()
            .HasOne(item => item.Vehicle)
            .WithMany(vehicle => vehicle.WorkItems)
            .HasForeignKey(item => item.VehicleId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<DiagnosticDocument>()
            .HasOne(document => document.Vehicle)
            .WithMany()
            .HasForeignKey(document => document.VehicleId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<ServiceItem>().Property(service => service.Price).HasPrecision(12, 2);
        modelBuilder.Entity<WorkItem>().Property(work => work.Price).HasPrecision(12, 2);
        modelBuilder.Entity<WorkOrder>().Property(order => order.TotalCost).HasPrecision(12, 2);
    }
}
