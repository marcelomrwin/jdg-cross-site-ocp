using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace EmployeeNetCoreApp.Model
{
    [Table("Employees")]
	public class Employee
	{
        [Key]
        public long EmployeeId { get; set; }
        [Required]
        public string FullName { get; set; }
        public string? Designation { get; set; }
        public string Department { get; set; }
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

