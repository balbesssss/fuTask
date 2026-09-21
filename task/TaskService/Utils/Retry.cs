using System.Text.Json;
using TaskService.Models;

namespace TaskService.Utils;

static public class Retry
{
    public async static Task SendWebHook(IHttpClientFactory httpFactory, JsonSerializerOptions jsonOptions, TaskItem task, ILogger<Program> logger)
    {
        for (int i = 0; i<3; i++)
        {
            try
            {
                var client = httpFactory.CreateClient("NotificationService");
                var response = await client.PostAsJsonAsync("/api/webhooks/task_created",task, jsonOptions);
                if (response.IsSuccessStatusCode)
                {
                    return ;
                }
                logger.LogWarning("Attempt {Attempt}: status {response}",i+1,response.StatusCode);
            }
            catch (Exception ex)
            {
                logger.LogError("Attempt: {Attempt} | Service is not responding: {Error}", i+1, ex.Message);
            }
            if (i<2)
            {
                await Task.Delay(1000*(i+1));
            }
        }
    }
}
