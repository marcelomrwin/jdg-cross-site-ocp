package models

import (
	"encoding/json"
	"go.mongodb.org/mongo-driver/bson/primitive"
	"time"
)

var (
	CREATED = "CREATED"
	UPDATED = "UPDATED"
	DELETED = "DELETED"
)

type Employee struct {
	EmployeeId  primitive.ObjectID `json:"employeeId,omitempty" bson:"_id,omitempty"`
	FullName    string             `json:"fullName,omitempty" bson:"fullName,omitempty" binding:"required"`
	Designation string             `json:"designation" bson:"designation,omitempty" binding:"required"`
	Department  string             `json:"department" bson:"department,omitempty" binding:"required"`
	CreateDate  time.Time          `json:"createDate,omitempty" bson:"createDate,omitempty"`
	CreatedBy   string             `json:"createBy,omitempty" bson:"createBy,omitempty"`
	UpdatedDate time.Time          `json:"updatedDate,omitempty" bson:"updatedDate,omitempty"`
	UpdatedBy   string             `json:"updatedBy,omitempty" bson:"updatedBy,omitempty"`
	Version     int                `json:"version" bson:"version,omitempty"`
	UUID        string             `json:"uuid" bson:"uuid,omitempty"`
}

type EmployeeDTO struct {
	FullName    string    `json:"fullName" binding:"required"`
	Designation string    `json:"designation" binding:"required"`
	Department  string    `json:"department" binding:"required"`
	CreateDate  time.Time `json:"createDate" binding:"required"`
	CreatedBy   string    `json:"createBy" binding:"required"`
	UpdatedDate time.Time `json:"updatedDate,omitempty"`
	UpdatedBy   string    `json:"updatedBy,omitempty"`
	Version     int       `json:"version" binding:"required"`
	UUID        string    `json:"uuid" binding:"required"`
	State       string    `json:"state" binding:"required"`
}

func (e EmployeeDTO) MarshalJSON() ([]byte, error) {

	type Alias EmployeeDTO
	return json.Marshal(&struct {
		*Alias
		CreateDate  string `json:"createDate,omitempty"`
		UpdatedDate string `json:"updatedDate,omitempty"`
	}{
		Alias:       (*Alias)(&e),
		CreateDate:  getDateValue(&e.CreateDate),
		UpdatedDate: getDateValue(&e.UpdatedDate),
	})

}

func (e *EmployeeDTO) UnmarshalJSON(data []byte) error {
	if len(string(data)) == 0 {
		return nil
	}

	var fakeDto struct {
		FullName    string `json:"fullName" binding:"required"`
		Designation string `json:"designation" binding:"required"`
		Department  string `json:"department" binding:"required"`
		CreateDate  string `json:"createDate,omitempty"`
		CreatedBy   string `json:"createBy" binding:"required"`
		UpdatedDate string `json:"updatedDate,omitempty"`
		UpdatedBy   string `json:"updatedBy,omitempty"`
		Version     int    `json:"version" binding:"required"`
		UUID        string `json:"uuid" binding:"required"`
		State       string `json:"state" binding:"required"`
	}

	if err := json.Unmarshal(data, &fakeDto); err != nil {
		return err
	}

	e.FullName = fakeDto.FullName
	e.Designation = fakeDto.Designation
	e.Department = fakeDto.Department
	e.CreateDate = *parseDate(fakeDto.CreateDate)
	e.CreatedBy = fakeDto.CreatedBy
	e.UpdatedDate = *parseDate(fakeDto.UpdatedDate)
	e.UpdatedBy = fakeDto.UpdatedBy
	e.Version = fakeDto.Version
	e.UUID = fakeDto.UUID
	e.State = fakeDto.State

	return nil
}

func getDateValue(date *time.Time) string {
	layout := "2006-01-02T15:04:05"
	if date == nil {
		return ""
	} else {
		return date.Format(layout)
	}
}

func parseDate(date string) *time.Time {
	layout := "2006-01-02T15:04:05"
	if len(date) == 0 {
		return nil
	}

	dt, _ := time.Parse(layout, date)

	return &dt
}

func FromDTOToEmployee(dto *EmployeeDTO) Employee {
	employee := Employee{
		FullName:    dto.FullName,
		Designation: dto.Designation,
		Department:  dto.Department,
		CreateDate:  dto.CreateDate,
		CreatedBy:   dto.CreatedBy,
		UpdatedDate: dto.UpdatedDate,
		UpdatedBy:   dto.UpdatedBy,
		Version:     dto.Version,
		UUID:        dto.UUID,
	}
	return employee
}

func FromEmployeeToDTO(employee Employee) EmployeeDTO {
	dto := EmployeeDTO{
		FullName:    employee.FullName,
		Designation: employee.Designation,
		Department:  employee.Department,
		CreateDate:  employee.CreateDate,
		CreatedBy:   employee.CreatedBy,
		UpdatedDate: employee.UpdatedDate,
		UpdatedBy:   employee.UpdatedBy,
		Version:     employee.Version,
		UUID:        employee.UUID,
	}
	return dto
}
