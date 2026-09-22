using System;

namespace TaskService.DTO;

public class ResponseUser
{
    public Guid UserId {get;set;}
    public string Name {get;set;} = string.Empty;
}
