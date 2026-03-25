using TaskBoard.Models;
using TaskBoard.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<ITaskService, InMemoryTaskService>();

var app = builder.Build();

app.UseDefaultFiles();
app.UseStaticFiles();

app.MapGet("/api/tasks", (ITaskService service) =>
{
    var tasks = service.GetAll(includeCompleted: true);
    return Results.Ok(tasks);
});

app.MapGet("/api/tasks/{id:int}", (int id, ITaskService service) =>
{
    var task = service.GetById(id);
    return task is null ? Results.NotFound() : Results.Ok(task);
});

app.MapGet("/api/tasks/summary", (ITaskService service) =>
{
    var summary = service.GetSummary(DateOnly.FromDateTime(DateTime.UtcNow));
    return Results.Ok(summary);
});

app.MapPost("/api/tasks", (CreateTaskRequest request, ITaskService service) =>
{
    try
    {
        var created = service.Create(request);
        return Results.Created($"/api/tasks/{created.Id}", created);
    }
    catch (ArgumentException ex)
    {
        return Results.ValidationProblem(new Dictionary<string, string[]>
        {
            ["request"] = [ex.Message]
        });
    }
});

app.MapPost("/api/tasks/{id:int}/complete", (int id, ITaskService service) =>
{
    return service.MarkCompleted(id)
        ? Results.NoContent()
        : Results.NotFound();
});

app.Run();