using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using StackExchange.Redis;
using System.Text.Json.Serialization;
using System.Text.Json;

namespace EmployeeNetCoreApp.Model
{

    public class EmployeeDTO
    {

        public static readonly string CREATED = "CREATED";
        public static readonly string UPDATED = "UPDATED";
        public static readonly string DELETED = "DELETED";

        public Employee ToEntity()
        {
            Employee employee = new Employee();
            employee.UUID = UUID;
            employee.FullName = FullName;
            employee.Designation = Designation;
            employee.Department = Department;
            employee.CreateDate = CreateDate;
            employee.CreatedBy = CreatedBy;
            employee.UpdatedDate = UpdatedDate;
            employee.UpdatedBy = UpdatedBy;
            employee.Version = Version;

            return employee;
        }

        public static EmployeeDTO FromEntity(Employee employee)
        {
            EmployeeDTO employeeDTO = new EmployeeDTO();
            employeeDTO.UUID = employee.UUID;
            employeeDTO.FullName = employee.FullName;
            employeeDTO.Designation = employee.Designation;
            employeeDTO.Department = employee.Department;
            employeeDTO.CreateDate = employee.CreateDate;
            employeeDTO.CreatedBy = employee.CreatedBy;
            employeeDTO.UpdatedDate = employee.UpdatedDate;
            employeeDTO.UpdatedBy = employee.UpdatedBy;
            employeeDTO.Version = employee.Version;

            return employeeDTO;
        }

        public string ToJson()
        {
            var options = new JsonSerializerOptions() { WriteIndented = true };
            options.Converters.Add(new CustomDateTimeConverter("yyyy-MM-ddTHH:mm:ss"));
            string json = JsonSerializer.Serialize(this, options);
            return json;
        }

        [JsonPropertyName("uuid")]
        public string? UUID { get; set; }
        [JsonPropertyName("fullName")]
        public string FullName { get; set; }
        [JsonPropertyName("designation")]
        public string? Designation { get; set; }
        [JsonPropertyName("department")]
        public string Department { get; set; }
        [JsonPropertyName("createDate")]
        public DateTime? CreateDate { get; set; }
        [JsonPropertyName("createBy")]
        public string? CreatedBy { get; set; }
        [JsonPropertyName("updatedDate")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public DateTime? UpdatedDate { get; set; }
        [JsonPropertyName("updatedBy")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? UpdatedBy { get; set; }
        [JsonPropertyName("version")]
        public int Version { get; set; }
        public string State { get; set; }
    }
}

