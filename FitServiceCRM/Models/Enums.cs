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

public enum WorkOrderStatus
{
    New,
    Diagnostics,
    AwaitingApproval,
    Approved,
    InProgress,
    Delayed,
    Completed,
    Canceled
}

public enum DocumentKind
{
    DiagnosticConclusion,
    WorkReport,
    Estimate,
    Invoice,
    AcceptanceAct
}
