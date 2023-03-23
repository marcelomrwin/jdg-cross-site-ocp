package routes

import (
	"EmployeeGolangApp/controllers"
	"github.com/gin-gonic/gin"
)

type EmployeeRouteController struct {
	employeeController controllers.EmployeeController
}

func NewEmployeeRouteController(employeeController controllers.EmployeeController) EmployeeRouteController {
	return EmployeeRouteController{employeeController}
}

func (r *EmployeeRouteController) EmployeeRoute(rg *gin.RouterGroup) {
	router := rg.Group("/employees")

	router.GET("/", r.employeeController.FindEmployees)
	router.GET("/:employeeId", r.employeeController.FindEmployeeById)
	router.POST("/", r.employeeController.CreateEmployee)
	router.PUT("/:employeeId", r.employeeController.UpdateEmployee)
	router.DELETE("/:employeeId", r.employeeController.DeleteEmployee)
	router.OPTIONS("/", r.employeeController.GetAllEmployeesKeysInCache)
	router.PUT("/fromcache/:employeeId", r.employeeController.UpdateEntityFromCache)
	router.POST("/fromcache/:uuid", r.employeeController.ImportEmployeeFromCache)
}
