using Lota.Data;
using Lota.Models;
using Lota.Views.Pages;
using System.Windows;
using System.Windows.Controls;

namespace Lota.Views
{
    public partial class MainWindow : Window
    {
        private User _currentUser;
        private bool _isLoaded = false;

        public MainWindow(User user)
        {
            InitializeComponent();
            _currentUser = user;
            txtUser.Text = $"{user.FullName}\n({user.Role})";
            this.Loaded += (s, e) =>
            {
                _isLoaded = true;
                LoadPage("Employees");
                UpdatePendingUsersCount();
            };
        }

        private void MenuListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (MenuListBox.SelectedItem is ListBoxItem item && item.Tag != null)
            {
                string tag = item.Tag.ToString();
                LoadPage(tag);
            }
        }
        public void UpdateCurrentUserInfo(User updatedUser)
        {
            _currentUser = updatedUser;
            txtUser.Text = $"{updatedUser.FullName}\n({updatedUser.Role})";
        }
        private void LoadPage(string pageTag)
        {
            if (ContentArea == null || lblPageTitle == null) return;
            switch (pageTag)
            {
                case "Employees":
                    ContentArea.Content = new EmployeesPage();
                    lblPageTitle.Text = "Сотрудники";
                    break;
                case "TimeLog":
                    ContentArea.Content = new TimeLogPage();
                    lblPageTitle.Text = "Журнал отработанного времени";
                    break;
                case "Timesheet":
                    ContentArea.Content = new TimesheetPage();
                    lblPageTitle.Text = "Табель (форма Т-13)";
                    break;
                case "Admin":
                    if (_currentUser.Role != "admin")
                    {
                        MessageBox.Show("Доступ только для администраторов.", "Ограничение");
                        return;
                    }
                    UpdatePendingUsersCount();
                    ContentArea.Content = new AdminPage(_currentUser); // передача текущего пользователя
                    lblPageTitle.Text = "Администрирование";
                    break;
                default:
                    break;
            }

        }
        public void UpdatePendingUsersCount()
        {
            DatabaseHelper db = new DatabaseHelper();
            int count = db.GetPendingUsersCount();
            if (pendingCountText != null)
            {
                pendingCountText.Text = count.ToString();
                pendingBadge.Visibility = count > 0 ? Visibility.Visible : Visibility.Collapsed;
                pendingBadge.UpdateLayout();
            }
            if (pendingBadge == null)
                MessageBox.Show("pendingBadge is null");
            if (pendingCountText == null)
                MessageBox.Show("pendingCountText is null");
        }
        private void BtnExit_Click(object sender, RoutedEventArgs e)
        {
            LoginWindow login = new LoginWindow();
            login.Show();
            this.Close();
        }
    }
}