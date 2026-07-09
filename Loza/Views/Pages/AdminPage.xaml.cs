using Lota.Data;
using Lota.Models;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace Lota.Views.Pages
{
    public partial class AdminPage : UserControl
    {
        private DatabaseHelper db = new DatabaseHelper();
        private List<User> _users;
        private bool _isLoaded = false;
        private User _currentUser; // текущий администратор

        public AdminPage(User currentUser)
        {
            InitializeComponent();
            _currentUser = currentUser;
            this.Loaded += OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            _isLoaded = true;
            cmbFilter.SelectedValue = "all";
            LoadUsers("all");
        }

        private void LoadUsers(string filter)
        {
            if (gridUsers == null || !_isLoaded)
                return;

            if (filter == "pending")
                _users = db.GetUsersByStatus(false);
            else if (filter == "approved")
                _users = db.GetUsersByStatus(true);
            else // all
                _users = db.GetUsersByStatus();

            gridUsers.ItemsSource = _users;
        }

        private void CmbFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!_isLoaded || gridUsers == null)
                return;
            if (e.OriginalSource != cmbFilter)
                return;
            if (cmbFilter.SelectedItem is ComboBoxItem item)
            {
                string tag = item.Tag.ToString();
                LoadUsers(tag);
            }
        }

        private void BtnApprove_Click(object sender, RoutedEventArgs e)
        {
            if (gridUsers.SelectedItem is User selectedUser)
            {
                if (selectedUser.IsApproved)
                {
                    MessageBox.Show("Пользователь уже активирован.", "Информация");
                    return;
                }

                if (MessageBox.Show($"Активировать пользователя {selectedUser.Username}?",
                    "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    if (db.ApproveUser(selectedUser.Id))
                    {
                        selectedUser.IsApproved = true;
                        gridUsers.Items.Refresh();
                        if (cmbFilter.SelectedValue?.ToString() == "pending")
                            LoadUsers("pending");
                        if (Window.GetWindow(this) is MainWindow main)
                            main.UpdatePendingUsersCount();
                    }
                    else
                        MessageBox.Show("Ошибка при активации.", "Ошибка");
                }
            }
            else
            {
                MessageBox.Show("Выберите пользователя для активации.", "Ошибка");
            }
        }

        private void BtnReject_Click(object sender, RoutedEventArgs e)
        {
            if (gridUsers.SelectedItem is User selectedUser)
            {
                if (MessageBox.Show($"Удалить пользователя {selectedUser.Username}?",
                    "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
                {
                    if (db.RejectUser(selectedUser.Id))
                    {
                        _users.Remove(selectedUser);
                        gridUsers.ItemsSource = null;
                        gridUsers.ItemsSource = _users;
                        if (Window.GetWindow(this) is MainWindow main)
                            main.UpdatePendingUsersCount();
                    }
                    else
                        MessageBox.Show("Ошибка при удалении.", "Ошибка");
                }
            }
            else
            {
                MessageBox.Show("Выберите пользователя для удаления.", "Ошибка");
            }
        }

        private void BtnEdit_Click(object sender, RoutedEventArgs e)
        {
            if (gridUsers.SelectedItem is User selectedUser)
            {
                var dialog = new UserEditWindow(selectedUser);
                dialog.Owner = Window.GetWindow(this);
                if (dialog.ShowDialog() == true)
                {
                    if (db.UpdateUser(selectedUser))
                    {
                        gridUsers.Items.Refresh();
                        if (Window.GetWindow(this) is MainWindow main)
                        {
                            main.UpdatePendingUsersCount();
                            if (selectedUser.Id == _currentUser.Id)
                                main.UpdateCurrentUserInfo(selectedUser);
                        }
                    }
                    else
                        MessageBox.Show("Ошибка при обновлении пользователя.", "Ошибка");
                }
            }
            else
            {
                MessageBox.Show("Выберите пользователя для редактирования.", "Ошибка");
            }
        }

        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (gridUsers.SelectedItem is User selectedUser)
            {
                if (selectedUser.Id == _currentUser.Id)
                {
                    MessageBox.Show("Нельзя удалить самого себя.", "Ошибка");
                    return;
                }

                if (MessageBox.Show($"Удалить пользователя {selectedUser.Username}?",
                    "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
                {
                    if (db.RejectUser(selectedUser.Id))
                    {
                        _users.Remove(selectedUser);
                        gridUsers.ItemsSource = null;
                        gridUsers.ItemsSource = _users;
                        if (Window.GetWindow(this) is MainWindow main)
                            main.UpdatePendingUsersCount();
                    }
                    else
                        MessageBox.Show("Ошибка при удалении.", "Ошибка");
                }
            }
            else
            {
                MessageBox.Show("Выберите пользователя для удаления.", "Ошибка");
            }
        }
    }
}