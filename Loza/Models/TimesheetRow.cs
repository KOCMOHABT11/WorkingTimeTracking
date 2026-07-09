using System.Collections.Generic;

namespace Lota.Models
{
    public class TimesheetRow
    {
        public int EmployeeId { get; set; }
        public string FullName { get; set; }
        public string Position { get; set; }
        public Dictionary<int, decimal?> DayHours { get; set; } = new Dictionary<int, decimal?>();
        public decimal Half1Hours { get; set; }
        public decimal Half2Hours { get; set; }
        public decimal TotalHours { get; set; }
        public string PersonnelNumber { get; set; }
    }
}