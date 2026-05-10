namespace FitServiceCRM.Models;

public static class DisplayExtensions
{
    public static string Label(this UserRole role) => role switch
    {
        UserRole.Admin => "Администратор",
        UserRole.Master => "Мастер-приемщик",
        UserRole.Manager => "Руководитель сервиса",
        UserRole.Client => "Клиент сервиса",
        _ => role.ToString()
    };

    public static string Label(this CustomerType type) => type switch
    {
        CustomerType.Individual => "Физическое лицо",
        CustomerType.Company => "Организация",
        _ => type.ToString()
    };

    public static string Label(this CustomerStatus status) => status switch
    {
        CustomerStatus.Active => "Активный",
        CustomerStatus.Potential => "Потенциальный",
        CustomerStatus.Suspended => "Приостановлен",
        CustomerStatus.Archived => "Архивный",
        _ => status.ToString()
    };

    public static string Label(this AppointmentStatus status) => status switch
    {
        AppointmentStatus.Created => "Создана",
        AppointmentStatus.Confirmed => "Подтверждена",
        AppointmentStatus.InWork => "В работе",
        AppointmentStatus.Completed => "Завершена",
        AppointmentStatus.Canceled => "Отменена",
        _ => status.ToString()
    };

    public static string Label(this WorkOrderStatus status) => status switch
    {
        WorkOrderStatus.Draft => "Черновик",
        WorkOrderStatus.AwaitingManagerApproval => "Ожидает согласования руководителя",
        WorkOrderStatus.AwaitingClientApproval => "Ожидает согласования клиента",
        WorkOrderStatus.NeedsChanges => "Требуются изменения",
        WorkOrderStatus.Approved => "Согласовано",
        WorkOrderStatus.InProgress => "В работе",
        WorkOrderStatus.Delayed => "Задержан",
        WorkOrderStatus.Completed => "Завершено",
        WorkOrderStatus.Canceled => "Отменено",
        _ => status.ToString()
    };

    public static string Label(this WorkItemStatus status) => status switch
    {
        WorkItemStatus.Planned => "Запланирована",
        WorkItemStatus.InProgress => "В работе",
        WorkItemStatus.AwaitingApproval => "На согласовании",
        WorkItemStatus.Completed => "Выполнена",
        WorkItemStatus.Canceled => "Отменена",
        _ => status.ToString()
    };

    public static string Label(this ApprovalRole role) => role switch
    {
        ApprovalRole.Client => "Клиент",
        ApprovalRole.ServiceManager => "Руководитель сервиса",
        ApprovalRole.Master => "Мастер-приемщик",
        _ => role.ToString()
    };

    public static string Label(this ApprovalDecision decision) => decision switch
    {
        ApprovalDecision.Pending => "Ожидает решения",
        ApprovalDecision.Approved => "Согласовано",
        ApprovalDecision.Rejected => "Отклонено",
        ApprovalDecision.NeedsChanges => "Нужны изменения",
        _ => decision.ToString()
    };

    public static string Label(this DiagnosticConclusionStatus status) => status switch
    {
        DiagnosticConclusionStatus.Draft => "Черновик",
        DiagnosticConclusionStatus.Formed => "Сформировано",
        DiagnosticConclusionStatus.CustomerAcknowledged => "Клиент ознакомлен",
        _ => status.ToString()
    };

    public static string Label(this NotificationKind kind) => kind switch
    {
        NotificationKind.WorkOrderApproval => "Согласование заказ-наряда",
        NotificationKind.AppointmentReminder => "Напоминание о записи",
        NotificationKind.WorkStatusChanged => "Изменение статуса работ",
        NotificationKind.DiagnosticFinished => "Диагностика завершена",
        NotificationKind.Message => "Сообщение",
        _ => kind.ToString()
    };

    public static string Label(this DocumentKind kind) => kind switch
    {
        DocumentKind.DiagnosticConclusion => "Заключение диагностики",
        DocumentKind.WorkReport => "Отчет о выполнении работ",
        DocumentKind.Estimate => "Смета",
        DocumentKind.Invoice => "Счет",
        DocumentKind.AcceptanceAct => "Акт приема-передачи",
        _ => kind.ToString()
    };

    public static string BadgeClass(this WorkOrderStatus status) => status switch
    {
        WorkOrderStatus.Draft => "badge badge-neutral",
        WorkOrderStatus.AwaitingManagerApproval => "badge badge-warn",
        WorkOrderStatus.AwaitingClientApproval => "badge badge-warn",
        WorkOrderStatus.NeedsChanges => "badge badge-danger",
        WorkOrderStatus.Approved => "badge badge-good",
        WorkOrderStatus.InProgress => "badge badge-info",
        WorkOrderStatus.Delayed => "badge badge-danger",
        WorkOrderStatus.Completed => "badge badge-done",
        WorkOrderStatus.Canceled => "badge badge-neutral",
        _ => "badge badge-neutral"
    };

    public static string BadgeClass(this DiagnosticConclusionStatus status) => status switch
    {
        DiagnosticConclusionStatus.Draft => "badge badge-neutral",
        DiagnosticConclusionStatus.Formed => "badge badge-info",
        DiagnosticConclusionStatus.CustomerAcknowledged => "badge badge-good",
        _ => "badge badge-neutral"
    };

    public static string Money(this decimal value) => string.Create(
        System.Globalization.CultureInfo.GetCultureInfo("ru-RU"),
        $"{value:N0} ₽");
}
