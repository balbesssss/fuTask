namespace TaskService.Models;

public class User
{

    public User()
    {
        
    }

    public User(Guid id, string name)
    {
        Id = id; Name = name;
    }
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;


}
