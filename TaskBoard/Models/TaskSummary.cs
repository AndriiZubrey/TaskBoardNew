namespace TaskBoard.Models;

public sealed record TaskSummary(
    int Total,
    int Completed,
    int Pending,
    int Overdue,
    decimal AveragePriority);