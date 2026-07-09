using Lota.Data;
using Lota.Models;
using System;
using System.Linq;
using System.Windows;

namespace Lota.Views
{
    public partial class TimeLogDialogWindow : Window
    {
        public TimeLogRecord TimeLog { get; private set; }
        private bool _isEditMode;
        private DatabaseHelper db = new DatabaseHelper();

        public TimeLogDialogWindow(TimeLogRecord existingRecord = null)
        {
            InitializeComponent();

            // Заполнить список сотрудников
            var employees = db.GetAllEmployees();
            cmbEmployee.ItemsSource = employees;

            if (existingRecord != null)
            {
                _isEditMode = true;
                lblTitle.Text = "Редактирование записи";
                TimeLog = existingRecord;

                // Установить выбранного сотрудника
                cmbEmployee.SelectedValue = existingRecord.EmployeeId;
                SetDateTimePickers(existingRecord.LoginTime, existingRecord.LogoutTime);
                txtNotes.Text = existingRecord.Notes ?? "";
            }
            else
            {
                _isEditMode = false;
                lblTitle.Text = "Новая запись";
                TimeLog = new TimeLogRecord();
                dpLoginDate.SelectedDate = DateTime.Today;
                tpLoginTime.SelectedTime = DateTime.Now;
            }
        }

        private void SetDateTimePickers(DateTime loginTime, DateTime? logoutTime)
        {
            dpLoginDate.SelectedDate = loginTime.Date;
            tpLoginTime.SelectedTime = loginTime;
            if (logoutTime.HasValue)
            {
                dpLogoutDate.SelectedDate = logoutTime.Value.Date;
                tpLogoutTime.SelectedTime = logoutTime.Value;
            }
        }

        private DateTime GetLoginDateTime()
        {
            DateTime date = dpLoginDate.SelectedDate ?? DateTime.Today;
            TimeSpan time = tpLoginTime.SelectedTime?.TimeOfDay ?? TimeSpan.Zero;
            return date.Date + time;
        }

        private DateTime? GetLogoutDateTime()
        {
            if (dpLogoutDate.SelectedDate == null && tpLogoutTime.SelectedTime == null)
                return null;

            DateTime date = dpLogoutDate.SelectedDate ?? DateTime.Today;
            TimeSpan time = tpLogoutTime.SelectedTime?.TimeOfDay ?? TimeSpan.Zero;
            return date.Date + time;
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            if (cmbEmployee.SelectedValue == null)
            {
                MessageBox.Show("Выберите сотрудника.");
                return;
            }
            if (dpLoginDate.SelectedDate == null)
            {
                MessageBox.Show("Укажите дату входа.");
                return;
            }

            TimeLog.EmployeeId = (int)cmbEmployee.SelectedValue;
            TimeLog.LoginTime = GetLoginDateTime();
            TimeLog.LogoutTime = GetLogoutDateTime();
            TimeLog.Notes = txtNotes.Text.Trim();
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