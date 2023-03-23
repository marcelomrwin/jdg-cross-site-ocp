package models

import (
	"encoding/json"
	"testing"
	"time"
)

func TestEmployeeDTO_MarshalJSON(t *testing.T) {
	now := time.Now().UTC()
	fullname := "Test Employee"
	var updatedDate time.Time

	dto := EmployeeDTO{
		FullName:    fullname,
		Designation: "test",
		Department:  "test",
		CreateDate:  now,
		CreatedBy:   "test",
		Version:     1,
		UUID:        "111-222-333-444",
		State:       "CREATED",
	}

	jsonData, err := json.Marshal(dto)
	if err != nil {
		t.Error("Error while converting JSON", err)
	}
	//assert createDate exists
	var dtoParsed EmployeeDTO
	if fails := json.Unmarshal(jsonData, &dtoParsed); fails != nil {
		t.Error("Error while restore Employee from json", fails)
	}

	if dtoParsed.UpdatedDate != updatedDate {
		t.Error("Updated date must be nil at this point")
	}

	t.Log(string(jsonData))

}
