
using System.Net.Http.Headers;
using TaskService.Desktop;
using TaskService.Models;

namespace TaskSerrvice.Desctop
{
    public partial class Action : Form
    {
        private static Action? _action = null;
        private static Guid Id { get; set; }
        private static TaskForm? _parent = null;
        public static EditForm? _edit = null;

        private Action(TaskForm parent)
        {
            _parent = parent;
            InitializeComponent();
        }

        public static Action GetAction(TaskForm parent,Point location, Guid id)
        {
            if (_action == null || _action.IsDisposed)
            {
                _action = new Action(parent);
            }
            _action.Focus();
            _action.Location = location;
            Id = id;
            return _action;
        }
        public void ShowForTask()
        {
            if (IsDisposed) return;
            if (!Visible) Show();
            else BringToFront();
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            _action = null;
            base.OnFormClosed(e);
            _parent?.ClearSelection();
            _edit?.Close();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            EditForm.GetEditForm(_action!, Id).ShowEdit();
        }

        private async void button2_Click(object sender, EventArgs e)
        {
            var requests = new HttpRequestMessage(HttpMethod.Delete, $"/api/task/{Id}");
            requests.Headers.Authorization = new AuthenticationHeaderValue("Bearer", Session.Token);
            var response = await Session.Client.SendAsync(requests);
            if (!response.IsSuccessStatusCode)
            {
                MessageBox.Show($"Ошибка: {response.StatusCode}");
                return;
            }
            MessageBox.Show("Удалено");
            TaskForm.dg?.Rows.Remove(TaskForm._row);
            return;
        }
    }
}
