using Lota.Data;
using Lota.Models;
using System.Windows;
using System.Windows.Controls;

namespace Lota.Views
{
    public partial class UserEditWindow : Window
    {
        private User _user;
        private DatabaseHelper db = new DatabaseHelper();

        public bool PasswordChanged { get; private set; } = false;

        public UserEditWindow(User user)
        {
            InitializeComponent();
            _user = user;

            txtUsername.Text = user.Username;
            txtFullName.Text = user.FullName;
            txtEmail.Text = user.Email;

            // Выбор роли
            foreach (ComboBoxItem item in cmbRole.Items)
            {
                if (item.Tag.ToString() == user.Role)
                {
                    cmbRole.SelectedItem = item;
                    break;
                }
            }
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtUsername.Text) ||
                string.IsNullOrWhiteSpace(txtFullName.Text) ||
                string.IsNullOrWhiteSpace(txtEmail.Text))
            {
                MessageBox.Show("Заполните логин, ФИО и email.", "Ошибка");
                return;
            }

            _user.Username = txtUsername.Text.Trim();
            _user.FullName = txtFullName.Text.Trim();
            _user.Email = txtEmail.Text.Trim();
            _user.Role = (cmbRole.SelectedItem as ComboBoxItem)?.Tag.ToString() ?? "user";

            // Если запрошена смена пароля и пароль не пуст
            if (chkChangePassword.IsChecked == true && !string.IsNullOrEmpty(txtNewPassword.Password))
            {
                if (db.ChangeUserPassword(_user.Id, txtNewPassword.Password))
                {
                    PasswordChanged = true;
                }
                else
                {
                    MessageBox.Show("Не удалось сменить пароль.", "Ошибка");
                    return;
                }
            }

            DialogResult = true;
            Close();
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}