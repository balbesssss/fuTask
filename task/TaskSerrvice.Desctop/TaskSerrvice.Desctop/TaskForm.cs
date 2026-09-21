using System.Net.Http.Headers;
using System.Net.Http.Json;
using TaskService.Desktop;
using TaskService.Models;

namespace TaskSerrvice.Desctop
{
    public partial class TaskForm : Form
    {
        public TaskForm()
        {
            InitializeComponent();
            Load += TaskFormLoad;
            dataGridView1.SelectionChanged += DataGridView1_SelectionChanged;
        }

        private void DataGridView1_SelectionChanged(object? sender, EventArgs e)
        {
            if (dataGridView1.SelectedRows.Count == 0) return;
            var row = dataGridView1.SelectedRows[0];
            var id = row.Cells["Id"].Value!.ToString();
            var title = row.Cells["Title"].Value!.ToString();
            var description = row.Cells["Description"].Value?.ToString();
            var status = row.Cells["Status"].Value!.ToString();
            var action = new Action(Guid.Parse(id!));
            action.Show();
        }

        private async void TaskFormLoad(object? sender, EventArgs e)
        {
            try
            {
                var requests = new HttpRequestMessage(HttpMethod.Get, "/api/task/");
                requests.Headers.Authorization = new AuthenticationHeaderValue("Bearer", Session.Token);
                var response = await Session.Client.SendAsync(requests);
                if (!response.IsSuccessStatusCode)
                {
                    MessageBox.Show($"Ошибка: {response.StatusCode}");
                    return;
                }
                var tasks = await response.Content.ReadFromJsonAsync<List<TaskItem>>();
                dataGridView1.AutoGenerateColumns = false;
                dataGridView1.DataSource = tasks;
                
            }
            catch (HttpRequestException ex)
            {
                MessageBox.Show($"Server is not availble {ex.Message}");
            }
        }


    }
}
