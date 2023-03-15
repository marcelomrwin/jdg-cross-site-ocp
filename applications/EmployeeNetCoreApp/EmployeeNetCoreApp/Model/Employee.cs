using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EmployeeNetCoreApp.Model
{
	[Table("Employees")]
	public class Employee
	{
        [Key]
        public int EmployeeId { get; set; }
        public string FullName { get; set; }
        public string Designation { get; set; }
        public int? Department { get; set; }
        public int? Status { get; set; }
        public DateTime? CreateDate { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public int? UpdatedBy { get; set; }
    }
}

