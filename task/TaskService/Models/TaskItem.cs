namespace TaskService.Models;

public enum StatusTasks
{   
    New,
    Inprogres,
    Done
}
public class TaskItem
{
    public Guid Id { get; set; }
    public string Title {get;set;} = string.Empty;
    public string? Description {get;set;}
    public StatusTasks Status {get;set;} = StatusTasks.New;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public Guid UserId{get;set;}
    public User? User{get;set;}
}
