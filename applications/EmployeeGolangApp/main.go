package main

import (
	"EmployeeGolangApp/controllers"
	"EmployeeGolangApp/database"
	"EmployeeGolangApp/routes"
	"EmployeeGolangApp/services"
	"context"
	"fmt"
	"github.com/caitlinelfring/go-env-default"
	"github.com/gin-contrib/cors"
	"github.com/gin-gonic/gin"
	"go.mongodb.org/mongo-driver/mongo"
	"go.mongodb.org/mongo-driver/mongo/readpref"
	"log"
	"strconv"
)

var (
	server      *gin.Engine
	ctx         context.Context
	mongoclient *mongo.Client

	employeeService         services.IEmployeeService
	employeeController      controllers.EmployeeController
	employeeCollection      *mongo.Collection
	employeeRouteController routes.EmployeeRouteController
)

func init() {

	ctx = context.TODO()

	// Connect to MongoDB
	mongoclient := database.ConnectDB()

	if err := mongoclient.Ping(ctx, readpref.Primary()); err != nil {
		panic(err)
	}

	fmt.Println("MongoDB successfully connected...")

	// Collections
	employeeCollection = database.GetCollection()
	employeeService = services.NewEmployeeService(employeeCollection, ctx)
	employeeController = controllers.NewEmployeeController(employeeService)
	employeeRouteController = routes.NewEmployeeRouteController(employeeController)
	server = gin.Default()
}

func main() {
	defer mongoclient.Disconnect(ctx)
	startGinServer()
}

func startGinServer() {

	corsConfig := cors.DefaultConfig()
	corsConfig.AllowOrigins = []string{"*"}
	corsConfig.AllowCredentials = true

	server.Use(cors.New(corsConfig))

	router := server.Group("/api")
	employeeRouteController.EmployeeRoute(router)
	port := env.GetIntDefault("PORT", 9000)
	log.Fatal(server.Run(":" + strconv.Itoa(port)))
}
