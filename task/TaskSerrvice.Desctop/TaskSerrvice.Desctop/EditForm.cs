using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Windows.Forms;
using TaskService.Desktop;
using TaskService.Models;

namespace TaskSerrvice.Desctop
{
    public partial class EditForm : Form
    {
        private Guid Id {  get; set; }
        public EditForm(Guid id)
        {
            Id = id;
            InitializeComponent();
        }

        private async void button1_Click(object sender, EventArgs e)
        {
            var title = textBox1.Text;
            var description = textBox2.Text;
            var requests = new HttpRequestMessage(HttpMethod.Patch, $"/api/task/{Id}");
            requests.Headers.Authorization = new AuthenticationHeaderValue("Bearer", Session.Token);
            var response = await Session.Client.SendAsync(requests);
            var result = await response.Content.ReadFromJsonAsync<TaskItem>();
            MessageBox.Show(result!.Title);
        }
    }
}
