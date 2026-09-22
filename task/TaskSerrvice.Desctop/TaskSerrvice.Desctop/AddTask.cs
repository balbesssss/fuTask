using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Net.Http.Json;
using System.Text;
using System.Windows.Forms;
using TaskService.Desktop;

namespace TaskSerrvice.Desctop
{
    public partial class AddTask : Form
    {

        public AddTask()
        {
            InitializeComponent();
        }

        private async void GetTask_Click(object sender, EventArgs e)
        {
            var title = textBox1.Text;
            var description = textBox2.Text;
            var request = new HttpRequestMessage(HttpMethod.Post, "/api/task/");
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", Session.Token);
            request.Content = JsonContent.Create(new { title, description }, options: Session.JsonOptions);
            var response = await Session.Client.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {
                MessageBox.Show("Заметка добавлена");
            }
            TaskForm.RefreshTask();

        }
    }
}
