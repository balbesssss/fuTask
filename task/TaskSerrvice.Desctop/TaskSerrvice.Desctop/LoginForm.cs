using System.Net.Http.Json;
using TaskSerrvice.Desctop;
using TaskService.Desktop;

namespace TaskSerrvice.Desctop
{
    public partial class LoginForm : Form
    {
        public LoginForm()
        {
            InitializeComponent();
        }
        private async void button1_Click(object sender, EventArgs e)
        {
            var name = textBox1.Text;
            var password = textBox2.Text;
            if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(password))
            {
                label5.Text = "Заполните поля";
                return;
            }
            var LoginData = new { name, password };
            var client = new HttpClient();
            var response = await Session.Client.PostAsJsonAsync("http://localhost:5176/api/auth/login", LoginData);
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<LoginResponse>();
                Session.Token = result!.Token;
                var taskForm = new TaskForm();
                taskForm.Show();
                Hide();
            }
            else
            {
                label5.Text = "Неверное имя или пароль";
            }
        }
    }
}
