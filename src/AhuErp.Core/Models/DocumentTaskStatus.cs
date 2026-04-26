namespace AhuErp.Core.Models
{
    /// <summary>
    /// Жизненный цикл поручения по документу.
    /// </summary>
    public enum DocumentTaskStatus
    {
        New = 0,
        InProgress = 1,
        OnReview = 2,
        Completed = 3,
        Cancelled = 4,
        Overdue = 5
    }
}
