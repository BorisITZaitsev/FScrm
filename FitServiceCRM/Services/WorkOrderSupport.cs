using FitServiceCRM.Models;
using FitServiceCRM.ViewModels;

namespace FitServiceCRM.Services;

public static class WorkOrderSupport
{
    private static readonly WorkOrderStatus[] ActiveVehicleStatuses =
    [
        WorkOrderStatus.AwaitingManagerApproval,
        WorkOrderStatus.AwaitingClientApproval,
        WorkOrderStatus.NeedsChanges,
        WorkOrderStatus.Approved,
        WorkOrderStatus.InProgress,
        WorkOrderStatus.Delayed
    ];

    public static bool HasActiveVehicleOrders(Vehicle vehicle) =>
        vehicle.OrderLinks.Any(link => link.WorkOrder is not null && ActiveVehicleStatuses.Contains(link.WorkOrder.Status))
        || vehicle.WorkOrders.Any(order => ActiveVehicleStatuses.Contains(order.Status));

    public static List<WorkOrderHistoryEntryViewModel> BuildHistory(WorkOrder order)
    {
        var events = new List<WorkOrderHistoryEntryViewModel>();

        events.AddRange(order.StatusHistory.Select(item => new WorkOrderHistoryEntryViewModel
        {
            At = item.ChangedAt,
            EventType = "Статус заказ-наряда",
            Author = item.ChangedByUser?.Login ?? "Система",
            Description = item.Comment,
            Result = item.NewStatus.Label()
        }));

        events.AddRange(order.ChangeRequests.Select(item => new WorkOrderHistoryEntryViewModel
        {
            At = item.CreatedAt,
            EventType = "Запрос на изменение",
            Author = item.AuthorUser?.Login ?? "Пользователь",
            Description = item.RequiredChanges,
            Result = item.Status.ToString()
        }));

        events.AddRange(order.Approvals.Select(item => new WorkOrderHistoryEntryViewModel
        {
            At = item.DecidedAt ?? item.CreatedAt,
            EventType = "Согласование",
            Author = item.ApproverUser?.Login ?? item.ApproverRole.Label(),
            Description = string.IsNullOrWhiteSpace(item.DecisionComment)
                ? $"Роль согласования: {item.ApproverRole.Label()}."
                : item.DecisionComment,
            Result = item.Decision.Label()
        }));

        events.AddRange(order.DiagnosticConclusions.Select(item => new WorkOrderHistoryEntryViewModel
        {
            At = item.CreatedAt,
            EventType = "Диагностика",
            Author = item.ReceiverEmployee?.FullName ?? "Мастер-приемщик",
            Description = item.DiagnosticResult,
            Result = item.Status.Label()
        }));

        events.AddRange(order.Attachments.Select(item => new WorkOrderHistoryEntryViewModel
        {
            At = item.UploadedAt,
            EventType = "Вложение",
            Author = item.UploadedByUser?.Login ?? "Система",
            Description = item.FileName,
            Result = item.ContentType
        }));

        return events
            .OrderByDescending(item => item.At)
            .ToList();
    }

    public static Notification CreateCustomerNotification(
        Customer customer,
        string title,
        string message,
        NotificationKind kind,
        WorkOrder? order = null,
        DiagnosticConclusion? diagnostic = null,
        Vehicle? vehicle = null,
        int? appUserId = null)
    {
        return new Notification
        {
            Customer = customer,
            AppUserId = appUserId,
            WorkOrder = order,
            DiagnosticConclusion = diagnostic,
            Vehicle = vehicle,
            Kind = kind,
            Channel = NotificationChannel.System,
            DeliveryStatus = NotificationDeliveryStatus.Sent,
            Title = title,
            Message = message,
            CreatedAt = DateTime.Now,
            SentAt = DateTime.Now
        };
    }

    public static string CustomerDisplayName(Customer customer) =>
        customer.Type == CustomerType.Company
            ? customer.LegalProfile?.OrganizationName ?? customer.Name
            : customer.IndividualProfile?.FullName ?? customer.Name;
}
