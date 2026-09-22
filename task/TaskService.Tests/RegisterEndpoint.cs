using System;
using System.Net;
using System.Net.Http.Json;
using TaskService.DTO;

namespace TaskService.Tests;

public class RegisterEndoint : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public RegisterEndoint(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }
    
    [Fact]
    public async Task RegisterValidationReturnCreated()
    {
        var user = new CreateUser
        {
            Name="test", Password="123qweasd"
        };
        var request = await _client.PostAsJsonAsync("/api/auth/register/",user);
        Assert.Equal(HttpStatusCode.Created, request.StatusCode);
    }

}