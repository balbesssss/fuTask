using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using TaskService.DTO;
using TaskService.Tests.Token;
using System.Net.Http.Headers;


namespace TaskService.Tests;

public class Tasks : IClassFixture<CustomWebApplicationFactory>,IAsyncLifetime
{
    private readonly HttpClient _client;
    private readonly JsonSerializerOptions _json;
    private string _token1;

    public Tasks(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
        _json = factory.GetService<JsonSerializerOptions>();

    }

    [Fact]
    public async Task CreateTaskWithUser1()
    {
        var task = new CreateTask("123","123");
        var createTask = new HttpRequestMessage(HttpMethod.Post,"/api/task");
        createTask.Headers.Authorization = new AuthenticationHeaderValue("Bearer",_token1);
        createTask.Content = JsonContent.Create(task,options: _json);
        var result = await _client.SendAsync(createTask);
        Assert.Equal(HttpStatusCode.Created, result.StatusCode);
    }

    public Task DisposeAsync() => Task.CompletedTask;

    public async  Task InitializeAsync()
    {
        var user = new CreateUser{Name = Guid.NewGuid().ToString(), Password = "12345678"};
        await _client.PostAsJsonAsync("/api/auth/register/",user);
        var login =  await _client.PostAsJsonAsync("/api/auth/login/",user);
        var result = await login.Content.ReadFromJsonAsync<TokenResponse>();
        _token1 = result!.token!;
        
    }

    [Fact]
    public async Task UpdateTaskWithUser1()
    {
        var task = new CreateTask("1423","123");
        var createTask = new HttpRequestMessage(HttpMethod.Post,"/api/task");
        createTask.Headers.Authorization = new AuthenticationHeaderValue("Bearer",_token1);
        createTask.Content = JsonContent.Create(task,options: _json);
        var resultOfCreate = await _client.SendAsync(createTask);
        var taskid = await resultOfCreate.Content.ReadFromJsonAsync<ResponseTask>(options: _json);
        Assert.Equal(HttpStatusCode.Created, resultOfCreate.StatusCode);

        var updateTask = new HttpRequestMessage(HttpMethod.Patch,$"/api/task/{taskid.Id}");
        updateTask.Headers.Authorization = new AuthenticationHeaderValue("Bearer",_token1);
        updateTask.Content = JsonContent.Create(new { Title = "test"},options: _json);
        var resultOfUpdate = await _client.SendAsync(updateTask);
        Assert.Equal(HttpStatusCode.OK, resultOfUpdate.StatusCode);
    }
}
