using System;

namespace Lota.Models
{
    public class TimeLogRecord
    {
        public int Id { get; set; }
        public int EmployeeId { get; set; }
        public DateTime LoginTime { get; set; }
        public DateTime? LogoutTime { get; set; }
        public string Notes { get; set; }
        public string EmployeeFullName { get; set; }
    }
}