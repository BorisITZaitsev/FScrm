namespace FitServiceCRM.Models;

public enum UserRole
{
    Admin,
    Master,
    Manager,
    Client
}

public enum CustomerType
{
    Individual,
    Company
}

public enum CustomerStatus
{
    Active,
    Potential,
    Suspended,
    Archived
}

public enum WorkOrderStatus
{
    Draft,
    AwaitingManagerApproval,
    AwaitingClientApproval,
    NeedsChanges,
    Approved,
    InProgress,
    Delayed,
    Completed,
    Canceled
}

public enum AppointmentStatus
{
    Created,
    Confirmed,
    InWork,
    Completed,
    Canceled
}

public enum WorkItemStatus
{
    Planned,
    InProgress,
    AwaitingApproval,
    Completed,
    Canceled
}

public enum DiagnosticConclusionStatus
{
    Draft,
    Formed,
    CustomerAcknowledged
}

public enum ApprovalRole
{
    Client,
    ServiceManager,
    Master
}

public enum ApprovalDecision
{
    Pending,
    Approved,
    Rejected,
    NeedsChanges
}

public enum ChangeRequestStatus
{
    Open,
    InProgress,
    Closed,
    Rejected
}

public enum NotificationKind
{
    WorkOrderApproval,
    AppointmentReminder,
    WorkStatusChanged,
    DiagnosticFinished,
    WorkOrderChanged,
    Message
}

public enum NotificationChannel
{
    System,
    Email,
    Sms
}

public enum NotificationDeliveryStatus
{
    Created,
    Sent,
    Read,
    Failed
}

public enum AuditAction
{
    Created,
    Updated,
    Deleted,
    StatusChanged,
    Approved,
    Rejected
}

public enum DocumentKind
{
    DiagnosticConclusion,
    WorkReport,
    Estimate,
    Invoice,
    AcceptanceAct
}
