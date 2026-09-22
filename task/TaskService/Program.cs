using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using TaskService.Data;
using TaskService.DTO;
using TaskService.Models;
using TaskService.Utils;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Description = "JWT Authorization header using the Bearer scheme.",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = JwtBearerDefaults.AuthenticationScheme,
        BearerFormat = "JWT"
    });

    options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
    {
        [new OpenApiSecuritySchemeReference("Bearer", document)] = []
    });
});

builder.Services.AddSingleton(new JsonSerializerOptions
{
    PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
    Converters = { new JsonStringEnumConverter(JsonNamingPolicy.SnakeCaseLower) }
});

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.SnakeCaseLower));
});

builder.Services.AddHttpClient("NotificationService", client =>
{
    client.BaseAddress = new Uri("http://127.0.0.1:8000");
});

builder.Services.AddDbContext<AppDbContext>(option => option.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
        };
    });

builder.Services.AddAuthorization();

builder.Services.AddValidation();

builder.Services.AddLogging();

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

app.UseSwagger();
app.UseSwaggerUI();

app.MapGet("/api/task/", async(AppDbContext db, ClaimsPrincipal user) =>
{
    var userId = Guid.Parse(user.FindFirst(ClaimTypes.NameIdentifier)!.Value);
    var tasks = await db.Tasks
    .Where(u => u.UserId == userId)
    .Select(t => new ResponseTask
    {
        Id = t.Id,
        Title = t.Title,
        Description = t.Description,
        Status = t.Status,
        CreatedAt = t.CreatedAt,
        UserId = t.UserId
    })
    .ToArrayAsync();
    return tasks;
}).RequireAuthorization();

app.MapPost("/api/task/", async (
    ClaimsPrincipal user,CreateTask createTask,
    AppDbContext db,IHttpClientFactory httpFactory,
    JsonSerializerOptions jsonOptions, ILogger<Program> logger) =>
{
    var UserId = Guid.Parse(user.FindFirst(ClaimTypes.NameIdentifier)!.Value);
    var NewTask = new TaskItem{
        Id = Guid.NewGuid(), Title = createTask.Title,
        Description = createTask.Description,CreatedAt = DateTime.UtcNow,  
        Status = StatusTasks.New, UserId = UserId};
    db.Tasks.Add(NewTask);
    await db.SaveChangesAsync();
    logger.LogInformation("Task {ID} was created by user = {USER}", NewTask.Id, UserId);
    _ =  Retry.SendWebHook(httpFactory,jsonOptions,NewTask, logger);
    return Results.Created($"/api/task/{NewTask.Id}", new ResponseTask(NewTask));
}).RequireAuthorization();

app.MapGet("/api/task/{id}", async (ClaimsPrincipal user,Guid id, AppDbContext db) => 
{
    var UserId = Guid.Parse(user.FindFirst(ClaimTypes.NameIdentifier)!.Value);
    var task = await db.Tasks
    .Where(t => t.Id == id && t.UserId == UserId)
    .Select(t => new ResponseTask { Id = t.Id, Title = t.Title, Description = t.Description, CreatedAt = t.CreatedAt, Status = t.Status, UserId = t.UserId })
    .FirstOrDefaultAsync();
    return task is null? Results.NotFound(): Results.Ok(task);
}).RequireAuthorization();

app.MapDelete("/api/task/{id}", async (ClaimsPrincipal user,Guid id, AppDbContext db, ILogger<Program> logger) =>
{
    var UserId = Guid.Parse(user.FindFirst(ClaimTypes.NameIdentifier)!.Value);
    var task = await db.Tasks.FirstOrDefaultAsync(t => t.Id == id && t.UserId == UserId);
    if (task is null)
    {
        return Results.NotFound();
    }
    db.Tasks.Remove(task);
    await db.SaveChangesAsync();
    logger.LogInformation("Task {TASK} is delete by {USER}",task.Id,UserId);
    return Results.NoContent();
}).RequireAuthorization();

app.MapPatch("/api/task/{id}",async (Guid id, EditTask editTask, ClaimsPrincipal user, AppDbContext db, ILogger<Program> logger) =>
{
    var UserId = Guid.Parse(user.FindFirst(ClaimTypes.NameIdentifier)!.Value);
    var task = await db.Tasks.FirstOrDefaultAsync(t => t.Id == id && t.UserId == UserId);
    var editType = typeof(EditTask);
    var taskType = typeof(TaskItem);
    foreach (var editProp in editType.GetProperties())
    {
        var value = editProp.GetValue(editTask);
        if (value == null)
        {
            continue;
        }
        var taskProp = taskType.GetProperty(editProp.Name);
        if (taskProp == null)
        {
            continue;
        }
        taskProp.SetValue(task, value);
    }
    await db.SaveChangesAsync();
    return Results.Ok(new ResponseTask(task!));
}).RequireAuthorization();


app.MapGet("/api/user/",async(AppDbContext db) => await db.Users.Select(u => new { u.Id, u.Name }).ToArrayAsync());

app.MapGet("/api/user/{id}",async (Guid id, AppDbContext db) =>
{
    var user = await db.Users
        .Where(u => u.Id == id)
        .Select(u => new { u.Id, u.Name })
        .FirstOrDefaultAsync();
    return user is null ? Results.NotFound() : Results.Ok(user);
});

app.MapPost("/api/auth/register/", async (CreateUser user,AppDbContext db, ILogger<Program> logger) =>
{
    var UserExists = await db.Users.FirstOrDefaultAsync(u => u.Name == user.Name);
    if (UserExists is not null)
    {
        logger.LogWarning("User with name {UserName} already exists", user.Name);
        return Results.Conflict("User already exists");
    }
    var NewUser = new User(Guid.NewGuid(),user.Name);
    NewUser.PasswordHash = PasswordHashVer.Hash(user.Password,NewUser);
    db.Users.Add(NewUser);
    await db.SaveChangesAsync();
    logger.LogInformation("New user {USER} is register in system ",NewUser.Id);
    return Results.Created($"/api/user/{NewUser.Id}", new { NewUser.Id, NewUser.Name });
});

app.MapPost("/api/auth/login/", async (LoginUser user,AppDbContext db, ILogger<Program> logger) =>
{
    var foundUser = await db.Users.FirstOrDefaultAsync(u => u.Name == user.Name);
    if (foundUser is null)
    {
        logger.LogWarning("Login attempt failed: user {UserName} not found", user.Name);
        return Results.Unauthorized();
    }
    if(!PasswordHashVer.Verify(user.Password, foundUser.PasswordHash, foundUser))
    {
        logger.LogWarning("User {ID} invalid password", foundUser.Id);
        return Results.Unauthorized();
    }
    var token = JWT.GenerateToken(foundUser,builder.Configuration);
    logger.LogInformation("User {USER} create token in {DATE}", foundUser.Id, DateTime.UtcNow);
    return Results.Ok(new { token });
});

app.Run();
public partial class Program { }