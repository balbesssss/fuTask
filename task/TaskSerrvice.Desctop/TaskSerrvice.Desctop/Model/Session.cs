using System.Text.Json;
using System.Text.Json.Serialization;

namespace TaskService.Desktop;

public static class Session
{
    public static string? Token { get; set; }
    public static HttpClient Client { get; } = new HttpClient
    {
        BaseAddress = new Uri("http://localhost:5176")
    };
    public static JsonSerializerOptions JsonOptions { get; } = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.SnakeCaseLower) }
    };
}