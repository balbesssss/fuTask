using System.Net.Http.Headers;
using System.Net.Http.Json;
using TaskService.Desktop;
using TaskService.Models;

namespace TaskSerrvice.Desctop
{
    public partial class TaskForm : Form
    {
        public static TaskForm? Instance { get; private set; }
        public TaskForm()
        {
            InitializeComponent();
            Load += TaskFormLoad;
            Instance = this;
            dataGridView1.SelectionChanged += DataGridView1_SelectionChanged;
        }

        public void ClearSelection()
        {
            dataGridView1.ClearSelection();
        }

        private void DataGridView1_SelectionChanged(object? sender, EventArgs e)
        {
            if (dataGridView1.SelectedRows.Count == 0) return;
            var row = dataGridView1.SelectedRows[0];
            var id = Guid.Parse(row.Cells["Id"].Value!.ToString()!);
            var title = row.Cells["Title"].Value!.ToString();
            var description = row.Cells["Description"].Value?.ToString();
            var status = row.Cells["Status"].Value!.ToString();
            var rowRect = dataGridView1.GetRowDisplayRectangle(row.Index, true);
            Point topRight = new Point(rowRect.Right + 570, rowRect.Top + 320);
            Point onForm = dataGridView1.PointToClient(dataGridView1.PointToScreen(topRight));
            Action.GetAction(Instance,onForm, id).ShowForTask();
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
                var tasks = await response.Content.ReadFromJsonAsync<List<TaskItem>>(Session.JsonOptions);
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
