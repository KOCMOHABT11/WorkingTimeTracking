using Lota.Data;
using Lota.Models;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Lota.Views.Pages
{
    public partial class EmployeesPage : UserControl
    {
        private DatabaseHelper db = new DatabaseHelper();
        private List<Employee> _allEmployees;

        public EmployeesPage()
        {
            InitializeComponent();
            LoadData();
        }

        private void LoadData()
        {
            _allEmployees = db.GetAllEmployees();
            ApplyFilter();
        }

        private void ApplyFilter()
        {
            string search = txtSearch.Text.ToLower();
            var filtered = _allEmployees
                .Where(e => string.IsNullOrEmpty(search) ||
                            e.LastName.ToLower().Contains(search) ||
                            e.FirstName.ToLower().Contains(search) ||
                            e.Position.ToLower().Contains(search))
                .ToList();
            gridEmployees.ItemsSource = filtered;
        }

        private void TxtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            ApplyFilter();
        }

        private void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            EmployeeDialogWindow dialog = new EmployeeDialogWindow();
            if (dialog.ShowDialog() == true)
            {
                Employee newEmp = dialog.Employee;
                if (db.AddEmployee(newEmp))
                {
                    LoadData();
                }
                else
                {
                    MessageBox.Show("Ошибка при добавлении сотрудника.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void BtnEdit_Click(object sender, RoutedEventArgs e)
        {
            if (gridEmployees.SelectedItem is Employee selected)
            {
                EmployeeDialogWindow dialog = new EmployeeDialogWindow(selected);
                if (dialog.ShowDialog() == true)
                {
                    Employee updated = dialog.Employee;
                    updated.Id = selected.Id; // сохраняем Id
                    if (db.UpdateEmployee(updated))
                    {
                        LoadData();
                    }
                    else
                    {
                        MessageBox.Show("Ошибка при обновлении данных.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (gridEmployees.SelectedItem is Employee selected)
            {
                if (MessageBox.Show($"Удалить сотрудника {selected.FullName}?", "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
                {
                    if (db.DeleteEmployee(selected.Id))
                    {
                        LoadData();
                    }
                    else
                    {
                        MessageBox.Show("Ошибка при удалении.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }
    }
}