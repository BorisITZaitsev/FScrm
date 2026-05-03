using FitServiceCRM.Data;
using FitServiceCRM.Models;
using Microsoft.EntityFrameworkCore;

namespace FitServiceCRM.Services;

public static class DemoSeeder
{
    public static async Task SeedAsync(AppDbContext db)
    {
        if (await db.Users.AnyAsync())
        {
            return;
        }

        var employees = new List<Employee>
        {
            new()
            {
                FullName = "Антонов Сергей Викторович",
                Phone = "+7 495 100-10-01",
                Email = "antonov@fitservice.local",
                Position = "Руководитель сервиса"
            },
            new()
            {
                FullName = "Лебедева Мария Игоревна",
                Phone = "+7 495 100-10-02",
                Email = "lebedeva@fitservice.local",
                Position = "Мастер-приемщик"
            },
            new()
            {
                FullName = "Ким Павел Олегович",
                Phone = "+7 495 100-10-03",
                Email = "kim@fitservice.local",
                Position = "Автомеханик"
            },
            new()
            {
                FullName = "Громов Илья Денисович",
                Phone = "+7 495 100-10-04",
                Email = "gromov@fitservice.local",
                Position = "Диагност"
            },
            new()
            {
                FullName = "Соколова Елена Андреевна",
                Phone = "+7 495 100-10-05",
                Email = "sokolova@fitservice.local",
                Position = "Администратор ИС"
            }
        };

        var customers = new List<Customer>
        {
            new()
            {
                Type = CustomerType.Individual,
                Name = "Никита Орлов",
                Phone = "+7 916 220-44-10",
                Email = "orlov@example.com",
                Address = "Москва, ул. Полярная, 12",
                TaxNumber = "770512345678",
                Vehicles =
                [
                    new Vehicle { Make = "Toyota", Model = "Camry", PlateNumber = "A123BC77", Year = 2019, Mileage = 86500 },
                    new Vehicle { Make = "Kia", Model = "Sportage", PlateNumber = "M453KA777", Year = 2021, Mileage = 52100 }
                ]
            },
            new()
            {
                Type = CustomerType.Company,
                Name = "ООО \"Альфа Логистика\"",
                Phone = "+7 495 744-18-32",
                Email = "fleet@alpha-logistics.example",
                Address = "Москва, 2-й Южнопортовый пр., 18",
                TaxNumber = "7723456789",
                Vehicles =
                [
                    new Vehicle { Make = "Ford", Model = "Transit", PlateNumber = "E901TT799", Year = 2020, Mileage = 142300 },
                    new Vehicle { Make = "Volkswagen", Model = "Caddy", PlateNumber = "K738MP799", Year = 2018, Mileage = 119800 }
                ]
            },
            new()
            {
                Type = CustomerType.Individual,
                Name = "Алина Кузнецова",
                Phone = "+7 926 540-72-90",
                Email = "kuznetsova@example.com",
                Address = "Одинцово, ул. Молодежная, 9",
                TaxNumber = "503201234567",
                Vehicles =
                [
                    new Vehicle { Make = "Hyundai", Model = "Solaris", PlateNumber = "P555OO50", Year = 2022, Mileage = 39100 }
                ]
            }
        };

        var services = new List<ServiceItem>
        {
            new() { Name = "Комплексная диагностика", Category = "Диагностика", Type = "Плановая", EstimatedDurationMinutes = 90, Price = 2900, ApproximatePurchaseIntervalDays = 180 },
            new() { Name = "Замена моторного масла", Category = "ТО", Type = "Регламентная", EstimatedDurationMinutes = 45, Price = 1850, ApproximatePurchaseIntervalDays = 120 },
            new() { Name = "Диагностика подвески", Category = "Диагностика", Type = "Ходовая часть", EstimatedDurationMinutes = 60, Price = 2100, ApproximatePurchaseIntervalDays = 240 },
            new() { Name = "Замена тормозных колодок", Category = "Ремонт", Type = "Тормозная система", EstimatedDurationMinutes = 80, Price = 4200, ApproximatePurchaseIntervalDays = 365 },
            new() { Name = "Шиномонтаж R16", Category = "Сезонные услуги", Type = "Колеса", EstimatedDurationMinutes = 50, Price = 3200, ApproximatePurchaseIntervalDays = 180 },
            new() { Name = "Замена свечей зажигания", Category = "ТО", Type = "Двигатель", EstimatedDurationMinutes = 70, Price = 3600, ApproximatePurchaseIntervalDays = 450 }
        };

        db.Employees.AddRange(employees);
        db.Customers.AddRange(customers);
        db.Services.AddRange(services);
        await db.SaveChangesAsync();

        var master = employees.Single(employee => employee.Position == "Мастер-приемщик");
        var mechanic = employees.Single(employee => employee.Position == "Автомеханик");
        var diagnostician = employees.Single(employee => employee.Position == "Диагност");

        var random = new Random(2026);
        var orders = new List<WorkOrder>();
        var start = new DateTime(DateTime.Today.Year, 1, 10, 9, 0, 0, DateTimeKind.Local).AddMonths(-11);

        for (var month = 0; month < 12; month++)
        {
            var count = 3 + month % 4 + random.Next(0, 3);

            for (var index = 0; index < count; index++)
            {
                var customer = customers[random.Next(customers.Count)];
                var vehicle = customer.Vehicles[random.Next(customer.Vehicles.Count)];
                var service = services[random.Next(services.Count)];
                var planned = start.AddMonths(month).AddDays(index * 4 + random.Next(0, 3)).AddHours(random.Next(0, 7));
                var isCompleted = month < 10 || index % 2 == 0;
                var total = service.Price + random.Next(0, 4) * 750;

                var order = new WorkOrder
                {
                    Customer = customer,
                    Vehicle = vehicle,
                    ReceiverEmployee = master,
                    Status = isCompleted ? WorkOrderStatus.Completed : index % 3 == 0 ? WorkOrderStatus.AwaitingApproval : WorkOrderStatus.InProgress,
                    CreatedAt = planned.AddDays(-2),
                    PlannedStartAt = planned,
                    CompletedAt = isCompleted ? planned.AddMinutes(service.EstimatedDurationMinutes + random.Next(10, 90)) : null,
                    PlannedDurationMinutes = service.EstimatedDurationMinutes,
                    ActualDurationMinutes = isCompleted ? service.EstimatedDurationMinutes + random.Next(10, 90) : null,
                    TotalCost = total,
                    CustomerComment = index % 2 == 0 ? "Проверить шум при движении и согласовать дополнительные работы." : "Плановое обслуживание.",
                    InternalComment = isCompleted ? "Работы закрыты, документы сформированы." : "Требуется контроль согласования.",
                    ClientApprovedAt = isCompleted ? planned.AddHours(-2) : null,
                    OrderServices = [new OrderService { ServiceItem = service }],
                    Executors =
                    [
                        new WorkOrderEmployee { Employee = mechanic },
                        new WorkOrderEmployee { Employee = diagnostician }
                    ],
                    WorkItems =
                    [
                        new WorkItem
                        {
                            ServiceItem = service,
                            Vehicle = vehicle,
                            Description = $"{service.Name}: выполнено по регламенту Fit Service",
                            Price = total
                        }
                    ],
                    SupportDocuments =
                    [
                        new SupportDocument
                        {
                            Kind = isCompleted ? DocumentKind.WorkReport : DocumentKind.Estimate,
                            FileName = $"FS-{planned:yyyyMM}-{month + 1:D2}{index + 1:D2}.pdf",
                            CreatedAt = planned
                        }
                    ]
                };

                if (service.Category == "Диагностика" || index % 3 == 0)
                {
                    order.DiagnosticDocuments.Add(new DiagnosticDocument
                    {
                        Employee = diagnostician,
                        Customer = customer,
                        Vehicle = vehicle,
                        PerformedAt = planned,
                        Summary = "Проведена первичная диагностика. Критических неисправностей не выявлено, рекомендованы регламентные работы.",
                        FileName = $"DIAG-{planned:yyyyMMdd}-{vehicle.PlateNumber}.pdf"
                    });
                }

                orders.Add(order);
            }
        }

        db.WorkOrders.AddRange(orders);
        await db.SaveChangesAsync();

        var activeOrder = await db.WorkOrders
            .Include(order => order.Customer)
            .OrderByDescending(order => order.PlannedStartAt)
            .FirstAsync(order => order.Status != WorkOrderStatus.Completed);

        db.Notifications.AddRange(
            new Notification
            {
                Customer = activeOrder.Customer,
                WorkOrder = activeOrder,
                Title = "Требуется согласование",
                Message = "Мастер-приемщик добавил работы и ожидает подтверждения клиента.",
                CreatedAt = DateTime.Now.AddHours(-5)
            },
            new Notification
            {
                Customer = customers[0],
                WorkOrder = orders.Last(order => order.Customer == customers[0]),
                Title = "Работы завершены",
                Message = "Заказ-наряд закрыт, отчет доступен в истории обслуживания.",
                CreatedAt = DateTime.Now.AddDays(-1),
                IsRead = true
            });

        db.Users.AddRange(
            new AppUser
            {
                Login = "admin",
                PasswordHash = PasswordService.Hash("Admin2026!"),
                Role = UserRole.Admin,
                Employee = employees.Single(employee => employee.Position == "Администратор ИС")
            },
            new AppUser
            {
                Login = "master",
                PasswordHash = PasswordService.Hash("Master2026!"),
                Role = UserRole.Master,
                Employee = master
            },
            new AppUser
            {
                Login = "manager",
                PasswordHash = PasswordService.Hash("Manager2026!"),
                Role = UserRole.Manager,
                Employee = employees.Single(employee => employee.Position == "Руководитель сервиса")
            },
            new AppUser
            {
                Login = "client",
                PasswordHash = PasswordService.Hash("Client2026!"),
                Role = UserRole.Client,
                Customer = customers[0]
            });

        await db.SaveChangesAsync();
    }
}
