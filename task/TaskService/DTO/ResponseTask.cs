using System.Runtime.InteropServices;
using TaskService.Models;

namespace TaskService.DTO;

public class ResponseTask
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public StatusTasks Status { get; set; }
    public DateTime CreatedAt  { get; set; }
    public Guid UserId { get; set; }
    
    public ResponseTask()
    {
        
    }
    public ResponseTask(TaskItem task)
    {
        Id = task.Id; Title = task.Title; Description = task.Description; Status = task.Status; CreatedAt = task.CreatedAt; UserId = task.UserId;
    }
}
