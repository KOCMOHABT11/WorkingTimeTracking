using System;

namespace Lota.Models
{
    public class Employee
    {
        public int Id { get; set; }
        public int? UserId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string MiddleName { get; set; }
        public string Position { get; set; }
        public string Department { get; set; }
        public DateTime HireDate { get; set; }
        public string Status { get; set; }
        public string FullName => $"{LastName} {FirstName} {MiddleName}".Trim();
    }
}