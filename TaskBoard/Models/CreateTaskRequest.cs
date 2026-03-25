namespace TaskBoard.Models;

public sealed record CreateTaskRequest(
    string Title,
    string? Description,
    DateOnly DueDate,
    int Priority);