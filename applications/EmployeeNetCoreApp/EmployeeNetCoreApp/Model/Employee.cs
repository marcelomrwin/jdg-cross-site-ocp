using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using StackExchange.Redis;
using System.Text.Json.Serialization;

namespace EmployeeNetCoreApp.Model
{
	[Table("Employees")]
	public class Employee
	{
        [Key]
        public int EmployeeId { get; set; }
        [Required]
        public string FullName { get; set; }
        public string? Designation { get; set; }
        public int Department { get; set; }
        public DateTime? CreateDate { get; set; }
        public string? CreatedBy { get; set; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public DateTime? UpdatedDate { get; set; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? UpdatedBy { get; set; }
        public int Version { get; set; }
        public string? UUID { get; set; }
    }
}

