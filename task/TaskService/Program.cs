using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using TaskService.Data;
using TaskService.DTO;
using TaskService.Models;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton(new JsonSerializerOptions
{
    PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
    Converters = { new JsonStringEnumConverter(JsonNamingPolicy.SnakeCaseLower) }
});

builder.Services.AddHttpClient("NotificationService", client =>
{
    client.BaseAddress = new Uri("http://127.0.0.1:8000");
});
builder.Services.AddDbContext<AppDbContext>(option => option.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.SnakeCaseLower));
});

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.MapGet("/api/task/", async(AppDbContext db) => await db.Tasks.ToArrayAsync());
app.MapPost("/api/task/", async (CreateTask createTask,AppDbContext db,IHttpClientFactory httpFactory, JsonSerializerOptions jsonOptions) =>
{
    var NewTask = new TaskItem{Id = Guid.NewGuid(), CreatedAt = DateTime.UtcNow, Title = createTask.Title, Description = createTask.Description, Status = TaskService.Models.TaskStatus.New};
    db.Tasks.Add(NewTask);
    await db.SaveChangesAsync();
    try
    {
        var client = httpFactory.CreateClient("NotificationService");
        await client.PostAsJsonAsync("api/webhooks/task_created",NewTask, jsonOptions);
    }
    catch
    {
        Console.WriteLine("Invalid send to service");
    }
    return Results.Created($"/api/task/{NewTask.Id}",NewTask);
});

app.MapGet("/api/task/{id}", async (Guid id, AppDbContext db) => 
{
   var task = await db.Tasks.FindAsync(id);
   return task is null? Results.NotFound(): Results.Ok(task);
});

app.MapDelete("/api/task/{id}", async (Guid id, AppDbContext db) =>
{
    var task = await db.Tasks.FindAsync(id);
    if (task is null)
    {
        return Results.NotFound();
    }
    db.Tasks.Remove(task);
    await db.SaveChangesAsync();
    return Results.NoContent();
});

app.MapPost("/api/user/",async (CreateUser user, AppDbContext db) =>
{
    var IsUserExists = await db.Users.FirstOrDefaultAsync(u => u.Name == user.Name);
    if (IsUserExists is not null )
    {
        return Results.BadRequest("User is already exists");
    }
    var NewUSer = new User{Id = Guid.NewGuid(), Name = user.Name};
    db.Users.Add(NewUSer);
    await db.SaveChangesAsync();
    return Results.Ok(NewUSer);
});

app.MapGet("/api/user/",async(AppDbContext db) => await db.Users.ToArrayAsync());



app.Run();
