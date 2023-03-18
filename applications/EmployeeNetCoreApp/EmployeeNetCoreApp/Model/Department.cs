using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EmployeeNetCoreApp.Model
{
	[Table("Departments")]
	public class Department
	{
        [Key]
        public int DepartmentId { get; set; }
        [Required]
        public string? DepartmentName { get; set; }
        [ForeignKey("Department")]
        public ICollection<Employee>? Employees { get; set; }
    }
}

