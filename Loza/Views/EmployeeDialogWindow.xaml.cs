using Lota.Models;
using System;
using System.Windows;
using System.Windows.Controls;

namespace Lota.Views
{
    public partial class EmployeeDialogWindow : Window
    {
        public Employee Employee { get; private set; }
        private bool _isEditMode;

        public EmployeeDialogWindow(Employee emp = null)
        {
            InitializeComponent();

            if (emp != null)
            {
                _isEditMode = true;
                lblTitle.Text = "Редактирование сотрудника";
                txtLastName.Text = emp.LastName;
                txtFirstName.Text = emp.FirstName;
                txtMiddleName.Text = emp.MiddleName ?? "";
                txtPosition.Text = emp.Position;
                txtDepartment.Text = emp.Department;
                dpHireDate.SelectedDate = emp.HireDate;
                // Выбрать статус
                foreach (ComboBoxItem item in cmbStatus.Items)
                {
                    if (item.Tag.ToString() == emp.Status)
                    {
                        cmbStatus.SelectedItem = item;
                        break;
                    }
                }
                Employee = emp; // сохраняем Id
            }
            else
            {
                _isEditMode = false;
                lblTitle.Text = "Добавление сотрудника";
                dpHireDate.SelectedDate = DateTime.Today;
                cmbStatus.SelectedIndex = 0; // активный по умолчанию
                Employee = new Employee();
            }
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            // Валидация
            if (string.IsNullOrWhiteSpace(txtLastName.Text) ||
                string.IsNullOrWhiteSpace(txtFirstName.Text) ||
                string.IsNullOrWhiteSpace(txtPosition.Text) ||
                string.IsNullOrWhiteSpace(txtDepartment.Text) ||
                dpHireDate.SelectedDate == null)
            {
                MessageBox.Show("Заполните обязательные поля: фамилия, имя, должность, отдел, дата.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            Employee.LastName = txtLastName.Text.Trim();
            Employee.FirstName = txtFirstName.Text.Trim();
            Employee.MiddleName = txtMiddleName.Text.Trim();
            Employee.Position = txtPosition.Text.Trim();
            Employee.Department = txtDepartment.Text.Trim();
            Employee.HireDate = dpHireDate.SelectedDate.Value;
            Employee.Status = (cmbStatus.SelectedItem as ComboBoxItem)?.Tag.ToString() ?? "active";

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