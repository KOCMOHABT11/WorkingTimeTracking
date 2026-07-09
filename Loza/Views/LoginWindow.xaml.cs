using Lota.Data;
using System.Windows;
using System.Windows.Controls;

namespace Lota.Views
{
    public partial class LoginWindow : Window
    {
        private DatabaseHelper dbHelper = new DatabaseHelper();
        private bool _isPasswordVisible = false;

        public LoginWindow()
        {
            InitializeComponent();
        }

        private void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            txtError.Text = "";
            string username = txtUsername.Text.Trim();
            string password = _isPasswordVisible ? txtPasswordVisible.Text : txtPasswordHidden.Password;
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                txtError.Text = "Заполните все поля.";
                return;
            }
            var (success, message, user) = dbHelper.Authenticate(username, password);
            if (success)
            {
                MainWindow main = new MainWindow(user);
                main.Show();
                this.Close();
            }
            else
            {
                txtError.Text = message;
            }
        }

        private void BtnGoToRegister_Click(object sender, RoutedEventArgs e)
        {
            RegistrationWindow regWin = new RegistrationWindow();
            regWin.Owner = this;
            regWin.ShowDialog();
        }

        // Методы для переключения видимости пароля
        private void BtnTogglePassword_Click(object sender, RoutedEventArgs e)
        {
            _isPasswordVisible = !_isPasswordVisible;
            var eyeText = btnTogglePassword.Template.FindName("eyeText", btnTogglePassword) as System.Windows.Controls.TextBlock;
            if (_isPasswordVisible)
            {
                txtPasswordVisible.Text = txtPasswordHidden.Password;
                txtPasswordVisible.Visibility = Visibility.Visible;
                txtPasswordHidden.Visibility = Visibility.Collapsed;
                if (eyeText != null) eyeText.Text = "🙈";
                btnTogglePassword.ToolTip = "Скрыть пароль";
            }
            else
            {
                txtPasswordHidden.Password = txtPasswordVisible.Text;
                txtPasswordVisible.Visibility = Visibility.Collapsed;
                txtPasswordHidden.Visibility = Visibility.Visible;
                if (eyeText != null) eyeText.Text = "👁";
                btnTogglePassword.ToolTip = "Показать пароль";
            }
        }

        private void PasswordHidden_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (!_isPasswordVisible)
                txtPasswordVisible.Text = txtPasswordHidden.Password;
        }

        private void PasswordVisible_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_isPasswordVisible)
                txtPasswordHidden.Password = txtPasswordVisible.Text;
        }
    }
}