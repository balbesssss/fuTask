using System.ComponentModel.DataAnnotations;

namespace TaskService.DTO;

public class CreateUser
{
    [Required][StringLength(100,MinimumLength = 1)]
    public string Name { get; set; } = string.Empty;
    [Required][StringLength(100, MinimumLength = 8)]
    public string Password { get; set; } = string.Empty;
}