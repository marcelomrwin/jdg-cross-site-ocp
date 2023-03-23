package controllers

import (
	"EmployeeGolangApp/models"
	"EmployeeGolangApp/services"
	"github.com/gin-gonic/gin"
	"net/http"
	"strings"
)

type EmployeeController struct {
	employeeService services.IEmployeeService
}

func NewEmployeeController(employeeService services.IEmployeeService) EmployeeController {
	return EmployeeController{employeeService}
}

func (controller *EmployeeController) CreateEmployee(ctx *gin.Context) {
	var employee *models.Employee

	if err := ctx.ShouldBindJSON(&employee); err != nil {
		ctx.JSON(http.StatusBadRequest, err.Error())
		return
	}

	newEmployee, err := controller.employeeService.SaveEmployee(employee)

	if err != nil {
		ctx.JSON(http.StatusBadGateway, gin.H{"status": "fail", "message": err.Error()})
		return
	}

	ctx.JSON(http.StatusCreated, newEmployee)
}

func (controller *EmployeeController) UpdateEmployee(ctx *gin.Context) {
	employeeId := ctx.Param("employeeId")

	var employee *models.Employee
	if err := ctx.ShouldBindJSON(&employee); err != nil {
		ctx.JSON(http.StatusBadGateway, gin.H{"status": "fail", "message": err.Error()})
		return
	}

	updatedEmployee, err := controller.employeeService.UpdateEmployee(employeeId, employee)
	if err != nil {

		if strings.Contains(err.Error(), "The local version") {
			ctx.JSON(http.StatusBadRequest, err.Error())
			return
		}

		ctx.JSON(http.StatusBadGateway, gin.H{"status": "fail", "message": err.Error()})
		return
	}

	ctx.JSON(http.StatusOK, updatedEmployee)
}

func (controller *EmployeeController) DeleteEmployee(ctx *gin.Context) {
	employeeId := ctx.Param("employeeId")

	err := controller.employeeService.DeleteEmployee(employeeId)

	if err != nil {
		ctx.JSON(http.StatusBadGateway, gin.H{"status": "fail", "message": err.Error()})
		return
	}

	ctx.JSON(http.StatusNoContent, nil)
}

func (controller *EmployeeController) FindEmployeeById(ctx *gin.Context) {
	employeeId := ctx.Param("employeeId")

	employee, err := controller.employeeService.GetEmployeeById(employeeId)

	if err != nil {
		if strings.Contains(err.Error(), "Id exists") {
			ctx.JSON(http.StatusNotFound, gin.H{"status": "fail", "message": err.Error()})
			return
		}
		ctx.JSON(http.StatusBadGateway, gin.H{"status": "fail", "message": err.Error()})
		return
	}

	ctx.JSON(http.StatusOK, employee)
}

func (controller *EmployeeController) FindEmployees(ctx *gin.Context) {

	employees, err := controller.employeeService.GetEmployees()
	if err != nil {
		ctx.JSON(http.StatusBadGateway, gin.H{"status": "fail", "message": err.Error()})
		return
	}

	ctx.JSON(http.StatusOK, employees)
}

func (controller *EmployeeController) GetAllEmployeesKeysInCache(ctx *gin.Context) {
	keysInCache, err := controller.employeeService.GetAllEmployeesKeysInCache()
	if err != nil {
		ctx.JSON(http.StatusBadGateway, gin.H{"status": "fail", "message": err.Error()})
		return
	}
	ctx.JSON(http.StatusOK, keysInCache)
}

func (controller *EmployeeController) UpdateEntityFromCache(ctx *gin.Context) {
	employeeId := ctx.Param("employeeId")
	err := controller.employeeService.UpdateEntityFromCache(employeeId)

	if err != nil {
		ctx.JSON(http.StatusBadGateway, gin.H{"status": "fail", "message": err.Error()})
		return
	}
	ctx.JSON(http.StatusOK, "Employee Updated successfully!")
}

func (controller *EmployeeController) ImportEmployeeFromCache(ctx *gin.Context) {
	uuid := ctx.Param("uuid")
	employeeId, err := controller.employeeService.ImportEntityFromCache(uuid)
	if err != nil {
		ctx.JSON(http.StatusBadGateway, gin.H{"status": "fail", "message": err.Error()})
		return
	}
	ctx.JSON(http.StatusCreated, "Employee id "+employeeId+" updated successfully!")
}
