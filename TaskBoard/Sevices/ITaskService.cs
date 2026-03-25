using TaskBoard.Models;

namespace TaskBoard.Services;

public interface ITaskService
{
    IReadOnlyList<TaskItem> GetAll(bool includeCompleted = true);
    TaskItem? GetById(int id);
    TaskItem Create(CreateTaskRequest request);
    bool MarkCompleted(int id);
    TaskSummary GetSummary(DateOnly today);
}