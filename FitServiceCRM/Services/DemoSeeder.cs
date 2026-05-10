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

        var serviceCenters = new List<ServiceCenter>
        {
            new()
            {
                Name = "Fit Service Москва Север",
                Address = "Москва, ул. Полярная, 12",
                Phone = "+7 495 100-10-00",
                Email = "msk-north@fitservice.local",
                IsActive = true
            },
            new()
            {
                Name = "Fit Service Одинцово",
                Address = "Одинцово, ул. Молодежная, 9",
                Phone = "+7 495 100-20-00",
                Email = "odintsovo@fitservice.local",
                IsActive = true
            }
        };

        var roles = Enum.GetValues<UserRole>()
            .Select(role => new SystemRole
            {
                Code = role.ToString(),
                Name = role.Label(),
                Description = $"Доступ для роли: {role.Label()}"
            })
            .ToList();

        var accessRights = new List<AccessRight>
        {
            new() { Code = "admin.users", ModuleName = "Пользователи", Description = "Управление учетными записями и ролями" },
            new() { Code = "admin.catalogs", ModuleName = "Справочники", Description = "Управление клиентами, сотрудниками, услугами и СТО" },
            new() { Code = "orders.manage", ModuleName = "Заказ-наряды", Description = "Создание и изменение заказ-нарядов" },
            new() { Code = "orders.approve", ModuleName = "Согласования", Description = "Согласование заказ-нарядов и запросов изменений" },
            new() { Code = "client.portal", ModuleName = "Клиентский кабинет", Description = "Личный кабинет клиента и уведомления" },
            new() { Code = "analytics.view", ModuleName = "Аналитика", Description = "Просмотр показателей сервиса" }
        };

        foreach (var role in roles)
        {
            var allowedRights = role.Code switch
            {
                nameof(UserRole.Admin) => accessRights,
                nameof(UserRole.Manager) => accessRights.Where(right => right.Code is "orders.approve" or "orders.manage" or "analytics.view" or "admin.catalogs"),
                nameof(UserRole.Master) => accessRights.Where(right => right.Code is "orders.manage" or "orders.approve" or "admin.catalogs"),
                _ => accessRights.Where(right => right.Code is "client.portal" or "orders.approve")
            };

            foreach (var right in allowedRights)
            {
                role.AccessRights.Add(new RoleAccessRight { SystemRole = role, AccessRight = right });
            }
        }

        var categories = new List<ServiceCategory>
        {
            new() { Name = "Диагностика", Description = "Проверка состояния автомобиля и подготовка заключения" },
            new() { Name = "ТО и жидкости", Description = "Регламентное техническое обслуживание" },
            new() { Name = "Ходовая часть", Description = "Ремонт подвески, рулевого управления и тормозной системы" },
            new() { Name = "Сезонные услуги", Description = "Шиномонтаж, кондиционер и подготовка к сезону" }
        };

        var services = new List<ServiceItem>
        {
            CreateService("Комплексная диагностика", categories[0], "Плановая", "Осмотр автомобиля, электронная диагностика и рекомендации.", 90, 2900, 180),
            CreateService("Диагностика подвески", categories[0], "Ходовая часть", "Проверка подвески, сайлентблоков, амортизаторов и тормозных узлов.", 60, 2100, 240),
            CreateService("Замена моторного масла", categories[1], "Регламентная", "Замена масла, фильтра и проверка технических жидкостей.", 45, 1850, 120),
            CreateService("Замена тормозных колодок", categories[2], "Тормозная система", "Замена передних или задних тормозных колодок.", 80, 4200, 365),
            CreateService("Шиномонтаж R16", categories[3], "Колеса", "Сезонная смена шин, балансировка и проверка давления.", 50, 3200, 180),
            CreateService("Заправка кондиционера", categories[3], "Климат", "Диагностика и заправка системы кондиционирования.", 70, 3600, 365)
        };

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
                Status = CustomerStatus.Active,
                Comment = "Постоянный клиент, предпочитает уведомления в личном кабинете.",
                Name = "Никита Орлов",
                Phone = "+7 916 220-44-10",
                Email = "orlov@example.com",
                IndividualProfile = new IndividualCustomer
                {
                    FullName = "Никита Орлов",
                    BirthDate = new DateOnly(1991, 4, 14),
                    Phone = "+7 916 220-44-10",
                    Email = "orlov@example.com"
                },
                Vehicles =
                [
                    new Vehicle { Vin = "JTNB11HK7K3001001", Make = "Toyota", Model = "Camry", PlateNumber = "A123BC77", Year = 2019, Mileage = 86500, Color = "Белый" },
                    new Vehicle { Vin = "XWEPC81ADN0002002", Make = "Kia", Model = "Sportage", PlateNumber = "M453KA777", Year = 2021, Mileage = 52100, Color = "Серый" }
                ]
            },
            new()
            {
                Type = CustomerType.Company,
                Status = CustomerStatus.Active,
                Comment = "Корпоративный парк, требуется прозрачное согласование работ.",
                Name = "ООО \"Альфа Логистика\"",
                Phone = "+7 495 744-18-32",
                Email = "fleet@alpha-logistics.example",
                Address = "Москва, 2-й Южнопортовый пр., 18",
                TaxNumber = "7723456789",
                LegalProfile = new LegalCustomer
                {
                    OrganizationName = "ООО \"Альфа Логистика\"",
                    Inn = "7723456789",
                    Kpp = "772301001",
                    Ogrn = "1127746123456",
                    LegalAddress = "Москва, 2-й Южнопортовый пр., 18",
                    ActualAddress = "Москва, 2-й Южнопортовый пр., 18",
                    Phone = "+7 495 744-18-32",
                    Email = "fleet@alpha-logistics.example"
                },
                Representatives =
                [
                    new CustomerRepresentative
                    {
                        FullName = "Смирнова Ольга Петровна",
                        Position = "Руководитель автопарка",
                        Phone = "+7 916 700-55-21",
                        Email = "smirnova@alpha-logistics.example"
                    },
                    new CustomerRepresentative
                    {
                        FullName = "Плотников Артем Сергеевич",
                        Position = "Логист",
                        Phone = "+7 916 700-55-22",
                        Email = "plotnikov@alpha-logistics.example"
                    }
                ],
                Vehicles =
                [
                    new Vehicle { Vin = "WF0XXXTTGXLL03003", Make = "Ford", Model = "Transit", PlateNumber = "E901TT799", Year = 2020, Mileage = 142300, Color = "Серебристый" },
                    new Vehicle { Vin = "WV1ZZZ2KZJX004004", Make = "Volkswagen", Model = "Caddy", PlateNumber = "K738MP799", Year = 2018, Mileage = 119800, Color = "Синий" }
                ]
            },
            new()
            {
                Type = CustomerType.Individual,
                Status = CustomerStatus.Potential,
                Comment = "Первичное обращение через сайт.",
                Name = "Алина Кузнецова",
                Phone = "+7 926 540-72-90",
                Email = "kuznetsova@example.com",
                IndividualProfile = new IndividualCustomer
                {
                    FullName = "Алина Кузнецова",
                    BirthDate = new DateOnly(1997, 10, 7),
                    Phone = "+7 926 540-72-90",
                    Email = "kuznetsova@example.com"
                },
                Vehicles =
                [
                    new Vehicle { Vin = "Z94CB41AAGR005005", Make = "Hyundai", Model = "Solaris", PlateNumber = "P555OO50", Year = 2022, Mileage = 39100, Color = "Красный" }
                ]
            }
        };

        db.ServiceCenters.AddRange(serviceCenters);
        db.Roles.AddRange(roles);
        db.AccessRights.AddRange(accessRights);
        db.ServiceCategories.AddRange(categories);
        db.Services.AddRange(services);
        db.Employees.AddRange(employees);
        db.Customers.AddRange(customers);
        await db.SaveChangesAsync();

        var adminRole = roles.Single(role => role.Code == nameof(UserRole.Admin));
        var managerRole = roles.Single(role => role.Code == nameof(UserRole.Manager));
        var masterRole = roles.Single(role => role.Code == nameof(UserRole.Master));
        var clientRole = roles.Single(role => role.Code == nameof(UserRole.Client));

        var adminUser = CreateUser("admin", "Admin2026!", UserRole.Admin, adminRole, employee: employees[4]);
        var masterUser = CreateUser("master", "Master2026!", UserRole.Master, masterRole, employee: employees[1]);
        var managerUser = CreateUser("manager", "Manager2026!", UserRole.Manager, managerRole, employee: employees[0]);
        var clientUser = CreateUser("client", "Client2026!", UserRole.Client, clientRole, customer: customers[0]);
        var fleetUser = CreateUser("fleet", "Fleet2026!", UserRole.Client, clientRole, customer: customers[1]);

        db.Users.AddRange(adminUser, masterUser, managerUser, clientUser, fleetUser);
        await db.SaveChangesAsync();

        employees[4].UserId = adminUser.Id;
        employees[1].UserId = masterUser.Id;
        employees[0].UserId = managerUser.Id;

        db.CustomerSystemAccesses.AddRange(
            new CustomerSystemAccess { Customer = customers[0], AppUser = clientUser },
            new CustomerSystemAccess { Customer = customers[1], AppUser = fleetUser, CustomerRepresentative = customers[1].Representatives[0] });

        await db.SaveChangesAsync();

        var random = new Random(2026);
        var orders = new List<WorkOrder>();
        var start = new DateTime(DateTime.Today.Year, 1, 10, 9, 0, 0, DateTimeKind.Local).AddMonths(-10);

        for (var month = 0; month < 10; month++)
        {
            var count = 3 + month % 3 + random.Next(0, 2);

            for (var index = 0; index < count; index++)
            {
                var customer = customers[random.Next(customers.Count)];
                var vehicle = customer.Vehicles[random.Next(customer.Vehicles.Count)];
                var service = services[random.Next(services.Count)];
                var center = serviceCenters[random.Next(serviceCenters.Count)];
                var planned = start.AddMonths(month).AddDays(index * 5 + random.Next(0, 3)).AddHours(random.Next(0, 7));
                var isCompleted = month < 8 || index % 2 == 0;
                var status = isCompleted
                    ? WorkOrderStatus.Completed
                    : index % 3 == 0
                        ? WorkOrderStatus.AwaitingClientApproval
                        : WorkOrderStatus.InProgress;
                var total = service.BasePrice + random.Next(0, 4) * 750;
                var plannedEnd = planned.AddMinutes(service.StandardDurationMinutes);
                DateTime? actualEnd = isCompleted ? plannedEnd.AddMinutes(random.Next(10, 90)) : null;

                var appointment = new ServiceAppointment
                {
                    Customer = customer,
                    Vehicle = vehicle,
                    ServiceCenter = center,
                    ReceiverEmployee = employees[1],
                    PlannedVisitAt = planned,
                    Status = isCompleted ? AppointmentStatus.Completed : AppointmentStatus.Confirmed,
                    CustomerComment = index % 2 == 0 ? "Проверить шум при движении и согласовать дополнительные работы." : "Плановое обслуживание.",
                    CreatedAt = planned.AddDays(-2),
                    Services = [new AppointmentService { ServiceItem = service }],
                    Vehicles = [new AppointmentVehicle { Vehicle = vehicle }]
                };

                var order = new WorkOrder
                {
                    OrderNumber = $"FS-{planned:yyyyMM}-{month + 1:D2}{index + 1:D2}",
                    ServiceAppointment = appointment,
                    Customer = customer,
                    Vehicle = vehicle,
                    ServiceCenter = center,
                    ReceiverEmployee = employees[1],
                    ServiceManagerEmployee = employees[0],
                    Status = status,
                    CreatedAt = planned.AddDays(-2),
                    UpdatedAt = actualEnd ?? planned,
                    PlannedStartAt = planned,
                    PlannedEndAt = plannedEnd,
                    ActualStartAt = isCompleted ? planned.AddMinutes(10) : null,
                    ActualEndAt = actualEnd,
                    CompletedAt = actualEnd,
                    PlannedDurationMinutes = service.StandardDurationMinutes,
                    ActualDurationMinutes = actualEnd is null ? null : (int)(actualEnd.Value - planned).TotalMinutes,
                    TotalCost = total,
                    CustomerComment = appointment.CustomerComment,
                    Comment = isCompleted ? "Работы закрыты, документы сформированы." : "Требуется контроль согласования.",
                    InternalComment = isCompleted ? "Работы закрыты, документы сформированы." : "Требуется контроль согласования.",
                    ClientApprovedAt = isCompleted ? planned.AddHours(-2) : null,
                    OrderServices =
                    [
                        new OrderService
                        {
                            Customer = customer,
                            ServiceItem = service
                        }
                    ],
                    VehiclesInOrder =
                    [
                        new WorkOrderVehicle { Vehicle = vehicle }
                    ],
                    Executors =
                    [
                        new WorkOrderEmployee { Customer = customer, Employee = employees[2] },
                        new WorkOrderEmployee { Customer = customer, Employee = employees[3] }
                    ],
                    WorkItems =
                    [
                        new WorkItem
                        {
                            ServiceItem = service,
                            Vehicle = vehicle,
                            ExecutorEmployee = employees[index % 2 == 0 ? 3 : 2],
                            Description = $"{service.Name}: выполнено по регламенту Fit Service",
                            Quantity = 1,
                            UnitPrice = total,
                            Discount = index % 4 == 0 ? 5 : 0,
                            TotalAmount = total,
                            Price = total,
                            Status = isCompleted ? WorkItemStatus.Completed : WorkItemStatus.InProgress,
                            PlannedStartAt = planned,
                            PlannedEndAt = plannedEnd,
                            ActualStartAt = isCompleted ? planned.AddMinutes(10) : null,
                            ActualEndAt = actualEnd
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
                    ],
                    StatusHistory =
                    [
                        new WorkOrderStatusHistory
                        {
                            ChangedByUser = masterUser,
                            NewStatus = status,
                            Comment = "Статус установлен мастером-приемщиком",
                            ChangedAt = planned
                        }
                    ]
                };

                if (service.ServiceCategory?.Name == "Диагностика" || index % 3 == 0)
                {
                    order.DiagnosticConclusions.Add(new DiagnosticConclusion
                    {
                        Customer = customer,
                        Vehicle = vehicle,
                        ReceiverEmployee = employees[3],
                        DiagnosticResult = "Проведена диагностика. Критических неисправностей не выявлено, есть рекомендации по регламентным работам.",
                        Recommendations = "Согласовать работы с клиентом и выполнить обслуживание в рамках текущего визита.",
                        Status = isCompleted ? DiagnosticConclusionStatus.CustomerAcknowledged : DiagnosticConclusionStatus.Formed,
                        CreatedAt = planned
                    });

                    order.DiagnosticDocuments.Add(new DiagnosticDocument
                    {
                        Employee = employees[3],
                        Customer = customer,
                        Vehicle = vehicle,
                        PerformedAt = planned,
                        Summary = "Диагностическое заключение сформировано на основании первичного осмотра.",
                        FileName = $"DIAG-{planned:yyyyMMdd}-{vehicle.PlateNumber}.pdf"
                    });
                }

                if (status == WorkOrderStatus.AwaitingClientApproval)
                {
                    var approver = customer == customers[0] ? clientUser : fleetUser;
                    order.Approvals.Add(new RoleApproval
                    {
                        ApproverUser = approver,
                        ApproverRole = ApprovalRole.Client,
                        Decision = ApprovalDecision.Pending,
                        DecisionComment = "Ожидается решение клиента."
                    });
                }

                if (customer.Type == CustomerType.Company && customer.Vehicles.Count > 1 && index % 2 == 0)
                {
                    var additionalVehicle = customer.Vehicles.First(candidate => candidate != vehicle);
                    order.VehiclesInOrder.Add(new WorkOrderVehicle { Vehicle = additionalVehicle });
                    order.WorkItems.Add(new WorkItem
                    {
                        ServiceItem = services[(index + 1) % services.Count],
                        Vehicle = additionalVehicle,
                        ExecutorEmployee = employees[2],
                        Description = $"Дополнительная работа по автомобилю {additionalVehicle.PlateNumber}",
                        Quantity = 1,
                        UnitPrice = services[(index + 1) % services.Count].BasePrice,
                        Discount = 0,
                        TotalAmount = services[(index + 1) % services.Count].BasePrice,
                        Price = services[(index + 1) % services.Count].BasePrice,
                        Status = isCompleted ? WorkItemStatus.Completed : WorkItemStatus.Planned,
                        PlannedStartAt = planned,
                        PlannedEndAt = planned.AddMinutes(services[(index + 1) % services.Count].StandardDurationMinutes),
                        ActualStartAt = isCompleted ? planned.AddMinutes(15) : null,
                        ActualEndAt = isCompleted ? actualEnd : null
                    });
                    order.TotalCost += services[(index + 1) % services.Count].BasePrice;
                }

                orders.Add(order);
            }
        }

        db.WorkOrders.AddRange(orders);
        await db.SaveChangesAsync();

        var fleetCustomer = customers[1];
        var fleetPrimaryVehicle = fleetCustomer.Vehicles[0];
        var fleetSecondaryVehicle = fleetCustomer.Vehicles[1];
        var diagnosticService = services[0];
        var maintenanceService = services[2];
        var suspensionService = services[1];
        var extraAppointment = new ServiceAppointment
        {
            Customer = fleetCustomer,
            Vehicle = fleetPrimaryVehicle,
            ServiceCenter = serviceCenters[0],
            ReceiverEmployee = employees[1],
            PlannedVisitAt = DateTime.Now.AddDays(3).Date.AddHours(11),
            Status = AppointmentStatus.Created,
            CustomerComment = "Комплексная запись по двум автомобилям корпоративного клиента.",
            CreatedAt = DateTime.Now.AddHours(-6),
            Services =
            [
                new AppointmentService { ServiceItem = diagnosticService },
                new AppointmentService { ServiceItem = maintenanceService }
            ],
            Vehicles =
            [
                new AppointmentVehicle { Vehicle = fleetPrimaryVehicle },
                new AppointmentVehicle { Vehicle = fleetSecondaryVehicle }
            ]
        };

        db.Appointments.Add(extraAppointment);

        var draftDiagnostic = new DiagnosticConclusion
        {
            Customer = customers[2],
            Vehicle = customers[2].Vehicles[0],
            ReceiverEmployee = employees[1],
            DiagnosticResult = "Черновик заключения по первичному осмотру.",
            Recommendations = "Требуется уточнить источник шума подвески.",
            Status = DiagnosticConclusionStatus.Draft,
            CreatedAt = DateTime.Now.AddDays(-1)
        };

        var formedDiagnostic = new DiagnosticConclusion
        {
            Customer = customers[0],
            Vehicle = customers[0].Vehicles[1],
            ReceiverEmployee = employees[1],
            DiagnosticResult = "Подтвержден износ передних тормозных колодок и масла ДВС.",
            Recommendations = "Рекомендуется заменить расходные материалы в ближайший визит.",
            Status = DiagnosticConclusionStatus.Formed,
            CreatedAt = DateTime.Now.AddHours(-10)
        };

        db.DiagnosticConclusions.AddRange(draftDiagnostic, formedDiagnostic);

        var scenarioBase = DateTime.Now.AddDays(-2);
        var statusScenarios = new[]
        {
            (WorkOrderStatus.Draft, "Черновик"),
            (WorkOrderStatus.AwaitingManagerApproval, "Ожидает согласования руководителя"),
            (WorkOrderStatus.NeedsChanges, "Требуются изменения"),
            (WorkOrderStatus.Approved, "Согласован"),
            (WorkOrderStatus.Delayed, "Задержан"),
            (WorkOrderStatus.Canceled, "Отменен")
        };

        for (var scenarioIndex = 0; scenarioIndex < statusScenarios.Length; scenarioIndex++)
        {
            var (scenarioStatus, scenarioLabel) = statusScenarios[scenarioIndex];
            var scenarioVehicle = scenarioIndex % 2 == 0 ? fleetPrimaryVehicle : fleetSecondaryVehicle;
            var scenarioService = scenarioIndex % 2 == 0 ? maintenanceService : suspensionService;
            var planned = scenarioBase.AddHours(scenarioIndex * 3);
            var scenarioOrder = new WorkOrder
            {
                OrderNumber = $"FS-DEMO-{scenarioIndex + 1:D3}",
                Customer = fleetCustomer,
                Vehicle = scenarioVehicle,
                ServiceCenter = serviceCenters[0],
                ReceiverEmployee = employees[1],
                ServiceManagerEmployee = employees[0],
                Status = scenarioStatus,
                CreatedAt = planned.AddHours(-4),
                UpdatedAt = planned,
                PlannedStartAt = planned,
                PlannedEndAt = planned.AddMinutes(scenarioService.StandardDurationMinutes),
                PlannedDurationMinutes = scenarioService.StandardDurationMinutes,
                TotalCost = scenarioService.BasePrice,
                CustomerComment = $"Демонстрационный сценарий: {scenarioLabel}.",
                Comment = $"Сценарий seed-данных: {scenarioLabel}.",
                InternalComment = $"Сценарий seed-данных: {scenarioLabel}.",
                OrderServices =
                [
                    new OrderService
                    {
                        Customer = fleetCustomer,
                        ServiceItem = scenarioService
                    }
                ],
                VehiclesInOrder =
                [
                    new WorkOrderVehicle { Vehicle = scenarioVehicle }
                ],
                WorkItems =
                [
                    new WorkItem
                    {
                        ServiceItem = scenarioService,
                        Vehicle = scenarioVehicle,
                        ExecutorEmployee = employees[2],
                        Description = $"Демонстрационная работа: {scenarioLabel}.",
                        Quantity = 1,
                        UnitPrice = scenarioService.BasePrice,
                        TotalAmount = scenarioService.BasePrice,
                        Price = scenarioService.BasePrice,
                        Status = scenarioStatus == WorkOrderStatus.Canceled ? WorkItemStatus.Canceled : scenarioStatus == WorkOrderStatus.Approved ? WorkItemStatus.Planned : WorkItemStatus.InProgress,
                        PlannedStartAt = planned,
                        PlannedEndAt = planned.AddMinutes(scenarioService.StandardDurationMinutes)
                    }
                ],
                StatusHistory =
                [
                    new WorkOrderStatusHistory
                    {
                        ChangedByUser = masterUser,
                        NewStatus = scenarioStatus,
                        Comment = $"Статус seed-сценария: {scenarioLabel}.",
                        ChangedAt = planned
                    }
                ],
                Approvals =
                [
                    new RoleApproval
                    {
                        ApproverUser = fleetUser,
                        ApproverRole = ApprovalRole.Client,
                        Decision = scenarioStatus == WorkOrderStatus.Approved ? ApprovalDecision.Approved : ApprovalDecision.Pending,
                        DecisionComment = "Seed-сценарий клиента."
                    },
                    new RoleApproval
                    {
                        ApproverUser = managerUser,
                        ApproverRole = ApprovalRole.ServiceManager,
                        Decision = scenarioStatus is WorkOrderStatus.Approved or WorkOrderStatus.AwaitingClientApproval ? ApprovalDecision.Approved : ApprovalDecision.Pending,
                        DecisionComment = "Seed-сценарий руководителя."
                    }
                ]
            };

            if (scenarioStatus == WorkOrderStatus.NeedsChanges)
            {
                scenarioOrder.ChangeRequests.Add(new ChangeRequest
                {
                    AuthorUser = fleetUser,
                    RecipientUser = masterUser,
                    RequiredChanges = "Просьба скорректировать состав работ и срок исполнения.",
                    Status = ChangeRequestStatus.Open,
                    CreatedAt = planned.AddHours(1)
                });
            }

            if (scenarioStatus == WorkOrderStatus.Delayed)
            {
                scenarioOrder.ActualStartAt = planned.AddMinutes(15);
            }

            db.WorkOrders.Add(scenarioOrder);
        }

        await db.SaveChangesAsync();

        var activeOrder = await db.WorkOrders
            .Include(order => order.Customer)
            .OrderByDescending(order => order.PlannedStartAt)
            .FirstAsync(order => order.Status != WorkOrderStatus.Completed);

        db.Notifications.AddRange(
            new Notification
            {
                AppUser = activeOrder.Customer == customers[0] ? clientUser : fleetUser,
                Customer = activeOrder.Customer!,
                WorkOrder = activeOrder,
                Kind = NotificationKind.WorkOrderApproval,
                Channel = NotificationChannel.System,
                DeliveryStatus = NotificationDeliveryStatus.Sent,
                Title = "Требуется согласование",
                Message = "Мастер-приемщик добавил работы и ожидает подтверждения клиента.",
                CreatedAt = DateTime.Now.AddHours(-5),
                SentAt = DateTime.Now.AddHours(-5)
            },
            new Notification
            {
                AppUser = clientUser,
                Customer = customers[0],
                WorkOrder = orders.Last(order => order.Customer == customers[0]),
                Kind = NotificationKind.WorkStatusChanged,
                Channel = NotificationChannel.System,
                DeliveryStatus = NotificationDeliveryStatus.Read,
                Title = "Работы завершены",
                Message = "Заказ-наряд закрыт, отчет доступен в истории обслуживания.",
                CreatedAt = DateTime.Now.AddDays(-1),
                SentAt = DateTime.Now.AddDays(-1),
                ReadAt = DateTime.Now.AddHours(-18),
                IsRead = true
            });

        db.ChangeRequests.Add(new ChangeRequest
        {
            WorkOrder = activeOrder,
            AuthorUser = activeOrder.Customer == customers[0] ? clientUser : fleetUser,
            RecipientUser = masterUser,
            RequiredChanges = "Просьба уточнить необходимость дополнительных работ и сроки выполнения.",
            Status = ChangeRequestStatus.Open,
            Messages =
            [
                new Message
                {
                    WorkOrder = activeOrder,
                    SenderUser = activeOrder.Customer == customers[0] ? clientUser : fleetUser,
                    RecipientUser = masterUser,
                    Subject = "Уточнение по заказ-наряду",
                    Body = "Прошу прислать подробное обоснование по рекомендованным работам.",
                    SentAt = DateTime.Now.AddHours(-3)
                }
            ]
        });

        db.AuditLogs.Add(new AuditLog
        {
            AppUser = adminUser,
            EntityName = nameof(Customer),
            EntityId = customers[0].Id,
            Action = AuditAction.Created,
            NewValue = "Демо-данные клиента созданы при первичной инициализации.",
            CreatedAt = DateTime.Now
        });

        await db.SaveChangesAsync();
    }

    private static ServiceItem CreateService(
        string name,
        ServiceCategory category,
        string type,
        string description,
        int duration,
        decimal price,
        int intervalDays)
    {
        return new ServiceItem
        {
            Name = name,
            ServiceCategory = category,
            Category = category.Name,
            Type = type,
            Description = description,
            StandardDurationMinutes = duration,
            EstimatedDurationMinutes = duration,
            BasePrice = price,
            Price = price,
            ApproximatePurchaseIntervalDays = intervalDays
        };
    }

    private static AppUser CreateUser(string login, string password, UserRole role, SystemRole systemRole, Employee? employee = null, Customer? customer = null)
    {
        return new AppUser
        {
            Login = login,
            PasswordHash = PasswordService.Hash(password),
            Role = role,
            Employee = employee,
            Customer = customer,
            IsActive = true,
            RoleAssignments =
            [
                new AppUserRole { SystemRole = systemRole }
            ]
        };
    }
}
