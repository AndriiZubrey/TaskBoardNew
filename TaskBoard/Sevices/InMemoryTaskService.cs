using TaskBoard.Models;
using TaskBoard.Services;

namespace TaskBoard.Services;

public sealed class InMemoryTaskService : ITaskService
{
    private readonly List<TaskItem> _items = [];
    private readonly object _sync = new();
    private int _nextId = 1;

    public InMemoryTaskService()
    {
        Seed();
    }

    public IReadOnlyList<TaskItem> GetAll(bool includeCompleted = true)
    {
        lock (_sync)
        {
            IEnumerable<TaskItem> query = _items;

            if (!includeCompleted)
            {
                query = query.Where(x => !x.IsCompleted);
            }

            return query
                .OrderBy(x => x.IsCompleted)
                .ThenBy(x => x.DueDate)
                .ThenByDescending(x => x.Priority)
                .ThenBy(x => x.Id)
                .Select(Clone)
                .ToList();
        }
    }

    public TaskItem? GetById(int id)
    {
        lock (_sync)
        {
            var item = _items.FirstOrDefault(x => x.Id == id);
            return item is null ? null : Clone(item);
        }
    }

    public TaskItem Create(CreateTaskRequest request)
    {
        Validate(request);

        lock (_sync)
        {
            var item = new TaskItem
            {
                Id = _nextId++,
                Title = request.Title.Trim(),
                Description = string.IsNullOrWhiteSpace(request.Description)
                    ? null
                    : request.Description.Trim(),
                DueDate = request.DueDate,
                Priority = request.Priority,
                IsCompleted = false,
                CompletedAtUtc = null
            };

            _items.Add(item);
            return Clone(item);
        }
    }

    public bool MarkCompleted(int id)
    {
        lock (_sync)
        {
            var item = _items.FirstOrDefault(x => x.Id == id);
            if (item is null || item.IsCompleted)
            {
                return false;
            }

            item.IsCompleted = true;
            item.CompletedAtUtc = DateTime.UtcNow;
            return true;
        }
    }

    public TaskSummary GetSummary(DateOnly today)
    {
        lock (_sync)
        {
            var total = _items.Count;
            var completed = _items.Count(x => x.IsCompleted);
            var pending = total - completed;
            var overdue = _items.Count(x => !x.IsCompleted && x.DueDate < today);
            var averagePriority = total == 0
                ? 0m
                : Math.Round((decimal)_items.Average(x => x.Priority), 2);

            return new TaskSummary(total, completed, pending, overdue, averagePriority);
        }
    }

    private static void Validate(CreateTaskRequest request)
    {
        if (request is null)
        {
            throw new ArgumentException("Request cannot be null.");
        }

        var title = request.Title?.Trim() ?? string.Empty;
        if (title.Length < 3 || title.Length > 80)
        {
            throw new ArgumentException("Title must be between 3 and 80 characters.");
        }

        if (request.Description is { Length: > 500 })
        {
            throw new ArgumentException("Description cannot be longer than 500 characters.");
        }

        if (request.Priority is < 1 or > 5)
        {
            throw new ArgumentException("Priority must be between 1 and 5.");
        }

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        //if (request.DueDate < today)
        //{
        //    throw new ArgumentException("Due date cannot be in the past.");
        //}
    }

    private void Seed()
    {
        Create(new CreateTaskRequest(
            "Prepare report",
            "Check data and finalize summary",
            DateOnly.FromDateTime(DateTime.UtcNow.AddDays(2)),
            4));

        Create(new CreateTaskRequest(
            "Fix login page",
            "Polish validation and error messages",
            DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)),
            5));

        Create(new CreateTaskRequest(
            "Review pull requests",
            null,
            DateOnly.FromDateTime(DateTime.UtcNow.AddDays(3)),
            3));
    }

    private static TaskItem Clone(TaskItem item) => new()
    {
        Id = item.Id,
        Title = item.Title,
        Description = item.Description,
        DueDate = item.DueDate,
        Priority = item.Priority,
        IsCompleted = item.IsCompleted,
        CompletedAtUtc = item.CompletedAtUtc
    };
}