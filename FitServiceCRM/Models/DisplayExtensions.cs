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

    public static string Label(this WorkOrderStatus status) => status switch
    {
        WorkOrderStatus.New => "Новая запись",
        WorkOrderStatus.Diagnostics => "Диагностика",
        WorkOrderStatus.AwaitingApproval => "Ожидает согласования",
        WorkOrderStatus.Approved => "Согласовано",
        WorkOrderStatus.InProgress => "В работе",
        WorkOrderStatus.Delayed => "Задержка",
        WorkOrderStatus.Completed => "Завершено",
        WorkOrderStatus.Canceled => "Отменено",
        _ => status.ToString()
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
        WorkOrderStatus.New => "badge badge-neutral",
        WorkOrderStatus.Diagnostics => "badge badge-info",
        WorkOrderStatus.AwaitingApproval => "badge badge-warn",
        WorkOrderStatus.Approved => "badge badge-good",
        WorkOrderStatus.InProgress => "badge badge-info",
        WorkOrderStatus.Delayed => "badge badge-danger",
        WorkOrderStatus.Completed => "badge badge-done",
        WorkOrderStatus.Canceled => "badge badge-neutral",
        _ => "badge badge-neutral"
    };

    public static string Money(this decimal value) => string.Create(
        System.Globalization.CultureInfo.GetCultureInfo("ru-RU"),
        $"{value:N0} ₽");
}
