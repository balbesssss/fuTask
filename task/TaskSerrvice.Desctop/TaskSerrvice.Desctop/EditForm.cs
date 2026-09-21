using Microsoft.VisualBasic.ApplicationServices;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows.Forms;
using TaskService.Desktop;
using TaskService.Models;

namespace TaskSerrvice.Desctop
{
    public partial class EditForm : Form
    {
        private static EditForm? _editForm = null;
        private static Guid Id { get; set; }
        public EditForm(Guid id, Action parent)
        {
            InitializeComponent();
        }

        private async void button1_Click(object sender, EventArgs e)
        {
            var title = textBox1.Text;
            var description = textBox2.Text;
            var requests = new HttpRequestMessage(HttpMethod.Patch, $"/api/task/{Id}");
            requests.Headers.Authorization = new AuthenticationHeaderValue("Bearer", Session.Token);
            requests.Content = JsonContent.Create(new { title, description }, options: Session.JsonOptions);
            try
            {
                var response = await Session.Client.SendAsync(requests);
                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync();
                    MessageBox.Show($"Ошибка: {response.StatusCode}\n{error}");
                    return;
                }
                var result = await response.Content.ReadFromJsonAsync<TaskItem>(Session.JsonOptions);
                if (result is null)
                {
                    MessageBox.Show("Сервер вернул пустой ответ");
                    return;
                }
                MessageBox.Show("Обновлено");

            }
            catch
            {
                MessageBox.Show("Ошибка");
            }
        }
        public static EditForm GetEditForm(Action parent, Guid id)
        {
            if (_editForm == null || _editForm.IsDisposed)
            {
                _editForm = new EditForm(id, parent);
                _editForm!.Location = new Point(parent.Location.X + parent.Width + 5, parent.Location.Y);
                Action._edit = _editForm;
            }
            _editForm.Focus();
            Id = id;
            return _editForm;
        }
        public void ShowEdit()
        {
            if (IsDisposed) return;
            if (!Visible) Show();
            else BringToFront();
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            _editForm = null;
            base.OnFormClosed(e);
            
        }
    }
}
