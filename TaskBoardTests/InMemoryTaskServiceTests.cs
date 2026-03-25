using TaskBoard.Models;
using TaskBoard.Services;
using Xunit;

public class InMemoryTaskServiceTests
{
    [Fact]
    public void Create_AddsTaskAndReturnsGeneratedId()
    {
        var service = new InMemoryTaskService();

        var created = service.Create(new CreateTaskRequest(
            "Write docs",
            "Prepare project notes",
            DateOnly.FromDateTime(DateTime.UtcNow.AddDays(2)),
            4));

        Assert.True(created.Id > 0);
        Assert.Equal("Write docs", created.Title);
        Assert.False(created.IsCompleted);
    }

    [Fact]
    public void Create_Throws_WhenPriorityIsOutOfRange()
    {
        var service = new InMemoryTaskService();

        var ex = Assert.Throws<ArgumentException>(() =>
            service.Create(new CreateTaskRequest(
                "Valid title",
                null,
                DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)),
                9)));

        Assert.Contains("Priority", ex.Message);
    }

    [Fact]
    public void MarkCompleted_ChangesStatus()
    {
        var service = new InMemoryTaskService();

        var task = service.Create(new CreateTaskRequest(
            "Complete me",
            null,
            DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)),
            3));

        var result = service.MarkCompleted(task.Id);

        Assert.True(result);
        Assert.True(service.GetById(task.Id)!.IsCompleted);
    }

    [Fact]
    public void GetSummary_CountsOverdueTasks()
    {
        var service = new InMemoryTaskService();

        service.Create(new CreateTaskRequest(
            "Overdue task",
            null,
            DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1)),
            2));

        var summary = service.GetSummary(DateOnly.FromDateTime(DateTime.UtcNow));

        Assert.True(summary.Total >= 4);
        Assert.True(summary.Overdue >= 1);
    }
}