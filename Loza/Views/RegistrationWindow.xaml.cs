using Lota.Data;
using System.Text.RegularExpressions;
using System.Windows;

namespace Lota.Views
{
    public partial class RegistrationWindow : Window
    {
        private DatabaseHelper dbHelper = new DatabaseHelper();

        public RegistrationWindow()
        {
            InitializeComponent();
        }

        private void BtnRegister_Click(object sender, RoutedEventArgs e)
        {
            txtMessage.Text = "";
            string username = txtUsername.Text.Trim();
            string fullName = txtFullName.Text.Trim();
            string email = txtEmail.Text.Trim();
            string password = txtPassword.Password;
            string confirmPassword = txtConfirmPassword.Password;

            // Валидация
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(fullName) ||
                string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                txtMessage.Text = "Все поля обязательны для заполнения.";
                txtMessage.Foreground = System.Windows.Media.Brushes.Red;
                return;
            }

            if (password != confirmPassword)
            {
                txtMessage.Text = "Пароли не совпадают.";
                txtMessage.Foreground = System.Windows.Media.Brushes.Red;
                return;
            }

            if (!Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
            {
                txtMessage.Text = "Некорректный email.";
                txtMessage.Foreground = System.Windows.Media.Brushes.Red;
                return;
            }

            bool result = dbHelper.RegisterUser(username, password, fullName, email);
            if (result)
            {
                txtMessage.Text = "Регистрация успешна. Ожидайте подтверждения администратором.";
                txtMessage.Foreground = System.Windows.Media.Brushes.SeaGreen;
                // Закрываем окно через 2 секунды
                var timer = new System.Windows.Threading.DispatcherTimer
                {
                    Interval = System.TimeSpan.FromSeconds(2)
                };
                timer.Tick += (s, args) =>
                {
                    timer.Stop();
                    this.Close();
                };
                timer.Start();
            }
            else
            {
                txtMessage.Text = "Пользователь с таким именем уже существует.";
                txtMessage.Foreground = System.Windows.Media.Brushes.Red;
            }
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}