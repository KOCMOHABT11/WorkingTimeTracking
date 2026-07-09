using Lota.Data;
using Lota.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Lota.Views.Pages
{
    public partial class TimeLogPage : UserControl
    {
        private DatabaseHelper db = new DatabaseHelper();
        private List<Employee> _employees;
        private List<TimeLogRecord> _allRecords;

        public TimeLogPage()
        {
            InitializeComponent();
            LoadEmployees();
            SetDefaultDates();
            LoadData();
        }

        private void LoadEmployees()
        {
            _employees = db.GetAllEmployees();
            cmbEmployee.Items.Clear();
            cmbEmployee.Items.Add(new { Text = "Все сотрудники", Value = (int?)null });
            foreach (var emp in _employees)
                cmbEmployee.Items.Add(new { Text = emp.FullName, Value = (int?)emp.Id });
            cmbEmployee.SelectedIndex = 0;
        }

        private void SetDefaultDates()
        {
            dpFrom.SelectedDate = DateTime.Today;
            dpTo.SelectedDate = DateTime.Today;
        }

        private void LoadData()
        {
            int? empId = (cmbEmployee.SelectedItem as dynamic)?.Value;
            DateTime? from = dpFrom.SelectedDate;
            DateTime? to = dpTo.SelectedDate;
            if (to == null && from != null) to = from.Value.Date.AddDays(1).AddSeconds(-1);
            if (from == null && to != null) from = to.Value.Date;
            if (from.HasValue) from = from.Value.Date;
            if (to.HasValue) to = to.Value.Date.AddDays(1).AddSeconds(-1); // конец дня

            _allRecords = db.GetTimeLogs(dateFrom: from, dateTo: to, employeeId: empId);
            gridTimeLog.ItemsSource = _allRecords;
        }

        private void FilterChanged(object sender, EventArgs e) => LoadData();

        private void BtnToday_Click(object sender, RoutedEventArgs e)
        {
            dpFrom.SelectedDate = DateTime.Today;
            dpTo.SelectedDate = DateTime.Today;
            cmbEmployee.SelectedIndex = 0;
            LoadData();
        }

        private void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new TimeLogDialogWindow();
            dialog.Owner = Window.GetWindow(this);
            if (dialog.ShowDialog() == true)
            {
                db.AddTimeLog(dialog.TimeLog);
                LoadData();
            }
        }

        private void MenuItemEdit_Click(object sender, RoutedEventArgs e)
        {
            if (gridTimeLog.SelectedItem is TimeLogRecord selected)
            {
                var dialog = new TimeLogDialogWindow(selected);
                dialog.Owner = Window.GetWindow(this);
                if (dialog.ShowDialog() == true)
                {
                    db.UpdateTimeLog(dialog.TimeLog);
                    LoadData();
                }
            }
        }

        private void MenuItemDelete_Click(object sender, RoutedEventArgs e)
        {
            if (gridTimeLog.SelectedItem is TimeLogRecord selected)
            {
                if (MessageBox.Show($"Удалить запись от {selected.LoginTime}?", "Подтверждение",
                    MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
                {
                    db.DeleteTimeLog(selected.Id);
                    LoadData();
                }
            }
        }

        private void CmbEmployee_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // защита от раннего срабатывания: если данные ещё не загружены (cmbEmployee.Items.Count == 0)
            if (cmbEmployee.Items.Count > 0)
                LoadData();
        }
    }
}