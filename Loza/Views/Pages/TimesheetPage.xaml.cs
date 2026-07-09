using ClosedXML.Excel;
using Lota.Data;
using Lota.Models;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
namespace Lota.Views.Pages
{
    public partial class TimesheetPage : UserControl
    {
        private DatabaseHelper db = new DatabaseHelper();
        private List<TimesheetRow> _currentData;
        private int _currentMonth, _currentYear;
        public TimesheetPage()
        {
            InitializeComponent();
            int curYear = DateTime.Now.Year;
            cmbYear.ItemsSource = Enumerable.Range(curYear - 2, 5).ToList();
            cmbYear.SelectedItem = curYear;
            cmbMonth.SelectedValue = DateTime.Now.Month;
        }
        private void BtnGenerate_Click(object sender, RoutedEventArgs e)
        {
            if (cmbYear.SelectedItem == null || cmbMonth.SelectedValue == null)
            {
                MessageBox.Show("Выберите год и месяц.");
                return;
            }
            _currentYear = Convert.ToInt32(cmbYear.SelectedItem);
            _currentMonth = Convert.ToInt32(cmbMonth.SelectedValue);
            _currentData = db.GetTimesheetData(_currentYear, _currentMonth);
            BuildGridColumns();
            gridTimesheet.ItemsSource = _currentData;
            UpdateTotalInfo();
        }
        private void BuildGridColumns()
        {
            gridTimesheet.Columns.Clear();
            int daysInMonth = DateTime.DaysInMonth(_currentYear, _currentMonth);
            gridTimesheet.Columns.Add(new DataGridTextColumn { Header = "№", Width = 30, Binding = new System.Windows.Data.Binding(".") { } });
            gridTimesheet.Columns.Add(new DataGridTextColumn { Header = "Сотрудник", Width = 150, Binding = new System.Windows.Data.Binding("FullName") });
            gridTimesheet.Columns.Add(new DataGridTextColumn { Header = "Таб. №", Width = 60, Binding = new System.Windows.Data.Binding("PersonnelNumber") });
            for (int day = 1; day <= 15; day++)
            {
                int d = day;
                gridTimesheet.Columns.Add(new DataGridTextColumn
                {
                    Header = day.ToString(),
                    Width = 70,
                    Binding = new System.Windows.Data.Binding($"DayHours[{d}]")
                    {
                        StringFormat = "{0:F1}",
                        TargetNullValue = "В"
                    }
                });
            }
            gridTimesheet.Columns.Add(new DataGridTextColumn { Header = "", Width = 10, IsReadOnly = true });
            for (int day = 16; day <= 31; day++)
            {
                if (day > daysInMonth)
                {
                    gridTimesheet.Columns.Add(new DataGridTextColumn { Header = day.ToString(), Width = 42, IsReadOnly = true });
                }
                else
                {
                    int d = day;
                    gridTimesheet.Columns.Add(new DataGridTextColumn
                    {
                        Header = day.ToString(),
                        Width = 70,
                        Binding = new System.Windows.Data.Binding($"DayHours[{d}]")
                        {
                            StringFormat = "{0:F1}",
                            TargetNullValue = "В"
                        }
                    });
                }
            }
            gridTimesheet.Columns.Add(new DataGridTextColumn { Header = "I пол.", Width = 70, Binding = new System.Windows.Data.Binding("Half1Hours") { StringFormat = "{0:F2}" } });
            gridTimesheet.Columns.Add(new DataGridTextColumn { Header = "II пол.", Width = 70, Binding = new System.Windows.Data.Binding("Half2Hours") { StringFormat = "{0:F2}" } });
            gridTimesheet.Columns.Add(new DataGridTextColumn { Header = "Месяц", Width = 80, Binding = new System.Windows.Data.Binding("TotalHours") { StringFormat = "{0:F2}" } });
        }
        private void UpdateTotalInfo()
        {
            if (_currentData == null) return;
            decimal total = _currentData.Sum(r => r.TotalHours);
            lblTotalInfo.Text = $"Всего отработано: {total:F2} часов по всем сотрудникам";
        }
        private void BtnExport_Click(object sender, RoutedEventArgs e)
        {
            if (_currentData == null || _currentData.Count == 0)
            {
                MessageBox.Show("Нет данных для экспорта.");
                return;
            }
            var dlg = new SaveFileDialog
            {
                Filter = "Excel files (*.xlsx)|*.xlsx",
                FileName = $"Табель_T13_{cmbMonth.Text}_{cmbYear.SelectedItem}.xlsx"
            };
            if (dlg.ShowDialog() == true)
            {
                ExportToExcel(dlg.FileName);
                MessageBox.Show($"Файл сохранён: {dlg.FileName}");
            }
        }
        private void ExportToExcel(string filePath)
        {
            string templatePath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Шаблон_Т13.xlsx");
            if (!System.IO.File.Exists(templatePath))
            {
                MessageBox.Show($"Файл шаблона не найден:\n{templatePath}\n\n", "Ошибка шаблона");
                return;
            }
            using (var wb = new XLWorkbook(templatePath))
            {
                var ws = wb.Worksheet(1);
                int daysInMonth = DateTime.DaysInMonth(_currentYear, _currentMonth);
                DateTime monthStart = new DateTime(_currentYear, _currentMonth, 1);
                string monthEndStr = monthStart.AddMonths(1).AddDays(-1).ToString("dd.MM.yyyy");
                ws.Cell(8, 1).Value = "ООО «Лоза»";
                ws.Cell(10, 1).Value = "Все сотрудники";
                ws.Cell("W14").Value = monthStart.ToString("MM");
                ws.Cell("Y14").Value = DateTime.Now.ToString("dd.MM.yyyy");
                ws.Cell("AB14").Value = monthStart.ToString("dd.MM.yyyy");
                ws.Cell("AD14").Value = monthEndStr;
                int startRow = 24;
                for (int i = 0; i < _currentData.Count; i++)
                {
                    var item = _currentData[i];
                    int r = startRow + i * 4;
                    ws.Cell(r, 1).Value = i + 1;
                    ws.Cell(r, 2).Value = $"{item.FullName}, {item.Position}";
                    ws.Cell(r, 3).Value = item.PersonnelNumber;
                    decimal half1 = 0;
                    for (int d = 1; d <= 15; d++)
                    {
                        int col = 3 + d;
                        if (d <= daysInMonth && item.DayHours.TryGetValue(d, out decimal? h) && h.HasValue && h > 0)
                        {
                            ws.Cell(r, col).Value = "Я";
                            half1 += h.Value;
                        }
                        else
                            ws.Cell(r, col).Value = "В";
                    }
                    ws.Cell(r, 19).Value = "Х";
                    for (int d = 1; d <= 15; d++)
                    {
                        int col = 3 + d;
                        if (d <= daysInMonth && item.DayHours.TryGetValue(d, out decimal? h) && h.HasValue)
                            ws.Cell(r + 1, col).Value = (double)h.Value;
                        else
                            ws.Cell(r + 1, col).Value = 0;
                    }
                    ws.Cell(r + 1, 19).Value = "Х";
                    ws.Cell(r + 1, 20).Value = (double)half1;
                    ws.Cell(r + 1, 21).Value = (double)item.TotalHours;
                    decimal half2 = 0;
                    for (int d = 16; d <= 31; d++)
                    {
                        int col = 4 + (d - 16);
                        if (d <= daysInMonth && item.DayHours.TryGetValue(d, out decimal? h) && h.HasValue && h > 0)
                        {
                            ws.Cell(r + 2, col).Value = "Я";
                            half2 += h.Value;
                        }
                        else
                            ws.Cell(r + 2, col).Value = "В";
                    }
                    ws.Cell(r + 2, 19).Value = "Х";
                    for (int d = 16; d <= 31; d++)
                    {
                        int col = 4 + (d - 16);
                        if (d <= daysInMonth && item.DayHours.TryGetValue(d, out decimal? h) && h.HasValue)
                            ws.Cell(r + 3, col).Value = (double)h.Value;
                        else
                            ws.Cell(r + 3, col).Value = 0;
                    }
                    ws.Cell(r + 3, 19).Value = "Х";
                    ws.Cell(r + 3, 20).Value = (double)half2;
                    ws.Cell(r + 3, 21).Value = 0;
                }
                wb.SaveAs(filePath);
            }
            MessageBox.Show($"Табель сохранён в {filePath}");
        }
        private void BtnExportCsv_Click(object sender, RoutedEventArgs e)
        {
            if (_currentData == null || _currentData.Count == 0)
            {
                MessageBox.Show("Нет данных для экспорта.");
                return;
            }
            var dlg = new Microsoft.Win32.SaveFileDialog
            {
                Filter = "CSV files (*.csv)|*.csv",
                FileName = $"Табель_{cmbMonth.Text}_{cmbYear.SelectedItem}_1C.csv"
            };
            if (dlg.ShowDialog() == true)
            {
                ExportToCsv1C(dlg.FileName);
                MessageBox.Show($"Файл сохранён: {dlg.FileName}");
            }
        }
        private void ExportToCsv1C(string filePath)
        {
            int daysInMonth = DateTime.DaysInMonth(_currentYear, _currentMonth);
            var sb = new System.Text.StringBuilder();
            sb.Append("ФИО;Таб.номер");
            for (int d = 1; d <= 31; d++)
                sb.Append($";{d}");
            sb.AppendLine(";Всего часов");
            foreach (var item in _currentData)
            {
                sb.Append($"{item.FullName};{item.PersonnelNumber}");
                for (int d = 1; d <= 31; d++)
                {
                    if (d <= daysInMonth && item.DayHours.TryGetValue(d, out decimal? h) && h.HasValue)
                        sb.Append($";{h.Value.ToString("F2", System.Globalization.CultureInfo.InvariantCulture)}");
                    else
                        sb.Append(";0");
                }
                sb.AppendLine($";{item.TotalHours.ToString("F2", System.Globalization.CultureInfo.InvariantCulture)}");
            }
            System.IO.File.WriteAllText(filePath, sb.ToString(), System.Text.Encoding.UTF8);
        }
        private void BtnPrint_Click(object sender, RoutedEventArgs e)
        {
            if (_currentData == null || _currentData.Count == 0)
            {
                MessageBox.Show("Нет данных для печати.");
                return;
            }
            int daysInMonth = DateTime.DaysInMonth(_currentYear, _currentMonth);
            FlowDocument doc = new FlowDocument
            {
                PageWidth = 29.7 * 96 / 2.19,
                PageHeight = 21.0 * 96 / 2.54,
                ColumnWidth = double.MaxValue,
                FontFamily = new FontFamily("Segoe UI"),
                FontSize = 9,
                PagePadding = new Thickness(20)
            };
            doc.Blocks.Add(new Paragraph(new Run($"Табель учёта рабочего времени за {cmbMonth.Text} {cmbYear.SelectedItem}"))
            {
                FontSize = 16,
                FontWeight = FontWeights.Bold,
                TextAlignment = TextAlignment.Center,
                Margin = new Thickness(0, 10, 0, 15)
            });
            Table table = new Table
            {
                CellSpacing = 0,
                BorderBrush = Brushes.Black,
                BorderThickness = new Thickness(1)
            };
            TableRowGroup rowGroup = new TableRowGroup();
            table.RowGroups.Add(rowGroup);
            table.Columns.Add(new TableColumn { Width = new GridLength(25) });
            table.Columns.Add(new TableColumn { Width = new GridLength(180) });
            table.Columns.Add(new TableColumn { Width = new GridLength(40) });  
            for (int d = 1; d <= daysInMonth; d++)
            {
                table.Columns.Add(new TableColumn { Width = new GridLength(28) }); 
            }
            table.Columns.Add(new TableColumn { Width = new GridLength(40) });  
            table.Columns.Add(new TableColumn { Width = new GridLength(40) });  
            table.Columns.Add(new TableColumn { Width = new GridLength(50) });  
            TableRow headerRow = new TableRow();
            headerRow.Cells.Add(CreateCell("№", true));
            headerRow.Cells.Add(CreateCell("Сотрудник", true));
            headerRow.Cells.Add(CreateCell("Таб. №", true));
            for (int d = 1; d <= daysInMonth; d++)
                headerRow.Cells.Add(CreateCell(d.ToString(), true));
            headerRow.Cells.Add(CreateCell("I пол.", true));
            headerRow.Cells.Add(CreateCell("II пол.", true));
            headerRow.Cells.Add(CreateCell("Месяц", true));
            rowGroup.Rows.Add(headerRow);
            int index = 1;
            foreach (var item in _currentData)
            {
                TableRow dataRow = new TableRow();
                dataRow.Cells.Add(CreateCell(index.ToString()));
                dataRow.Cells.Add(CreateCell($"{item.FullName}, {item.Position}"));
                dataRow.Cells.Add(CreateCell(item.PersonnelNumber));
                for (int d = 1; d <= daysInMonth; d++)
                {
                    if (item.DayHours.TryGetValue(d, out decimal? h) && h.HasValue)
                        dataRow.Cells.Add(CreateCell(h.Value.ToString("F1")));
                    else
                        dataRow.Cells.Add(CreateCell("В"));
                }
                dataRow.Cells.Add(CreateCell(item.Half1Hours.ToString("F1")));
                dataRow.Cells.Add(CreateCell(item.Half2Hours.ToString("F1")));
                dataRow.Cells.Add(CreateCell(item.TotalHours.ToString("F1")));
                rowGroup.Rows.Add(dataRow);
                index++;
            }
            doc.Blocks.Add(table);            
            PrintDialog printDialog = new PrintDialog();
            if (printDialog.ShowDialog() == true)
            {
                printDialog.PrintDocument(((IDocumentPaginatorSource)doc).DocumentPaginator, "Табель рабочего времени");
            }
            MessageBox.Show($"Успешная печать ");
        }        
        private TableCell CreateCell(string text, bool bold = false)
        {
            var p = new Paragraph(new Run(text))
            {
                Margin = new Thickness(1),
                FontSize = 8,
                TextAlignment = TextAlignment.Center
            };
            var cell = new TableCell(p);
            cell.BorderBrush = Brushes.Black;
            cell.BorderThickness = new Thickness(0.5);
            cell.Padding = new Thickness(1);
            cell.FontWeight = bold ? FontWeights.Bold : FontWeights.Normal;
            return cell;
        }
    }
}