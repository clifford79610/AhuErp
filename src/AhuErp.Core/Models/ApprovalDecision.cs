namespace AhuErp.Core.Models
{
    /// <summary>
    /// Решение согласующего на этапе маршрута.
    /// </summary>
    public enum ApprovalDecision
    {
        Pending = 0,
        Approved = 1,
        Rejected = 2,
        Comments = 3
    }

    /// <summary>
    /// Сводный статус прохождения документа по маршруту согласования.
    /// </summary>
    public enum ApprovalRouteStatus
    {
        Draft = 0,
        InProgress = 1,
        Completed = 2,
        Rejected = 3,
        Cancelled = 4
    }
}
