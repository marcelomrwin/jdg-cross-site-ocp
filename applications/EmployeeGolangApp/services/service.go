package services

import (
	"EmployeeGolangApp/cache"
	"EmployeeGolangApp/models"
	"context"
	"errors"
	"fmt"
	"github.com/caitlinelfring/go-env-default"
	"github.com/google/uuid"
	"go.mongodb.org/mongo-driver/bson"
	"go.mongodb.org/mongo-driver/bson/primitive"
	"go.mongodb.org/mongo-driver/mongo"
	"go.mongodb.org/mongo-driver/mongo/options"
	"strings"
	"time"
)

type IEmployeeService interface {
	GetEmployees() ([]*models.Employee, error)
	GetEmployeeById(string) (*models.Employee, error)
	GetEmployeeByUUID(string) (*models.Employee, error)
	SaveEmployee(*models.Employee) (*models.Employee, error)
	UpdateEmployee(string, *models.Employee) (*models.Employee, error)
	DeleteEmployee(string) error
	GetAllEmployeesKeysInCache() ([]string, error)
	UpdateEntityFromCache(string) error
	ImportEntityFromCache(string) (string, error)
}

type EmployeeService struct {
	employeeCollection *mongo.Collection
	ctx                context.Context
	dataGridRestClient cache.DataGridRestClient
}

func (e EmployeeService) GetEmployees() ([]*models.Employee, error) {
	query := bson.M{}

	cursor, err := e.employeeCollection.Find(e.ctx, query)
	if err != nil {
		return nil, err
	}

	defer cursor.Close(e.ctx)

	var employees []*models.Employee

	for cursor.Next(e.ctx) {
		employee := &models.Employee{}
		err := cursor.Decode(employee)

		if err != nil {
			return nil, err
		}

		employees = append(employees, employee)
	}

	if err := cursor.Err(); err != nil {
		return nil, err
	}

	if len(employees) == 0 {
		return []*models.Employee{}, nil
	}

	return employees, nil
}

func (e EmployeeService) GetEmployeeById(id string) (*models.Employee, error) {
	obId, _ := primitive.ObjectIDFromHex(id)

	query := bson.M{"_id": obId}

	var employee *models.Employee

	if err := e.employeeCollection.FindOne(e.ctx, query).Decode(&employee); err != nil {
		if err == mongo.ErrNoDocuments {
			return nil, errors.New("no document with that Id exists")
		}
		return nil, err
	}

	return employee, nil
}

func (e EmployeeService) GetEmployeeByUUID(uuid string) (*models.Employee, error) {
	query := bson.M{"uuid": uuid}

	var employee *models.Employee

	if err := e.employeeCollection.FindOne(e.ctx, query).Decode(&employee); err != nil {
		if err == mongo.ErrNoDocuments {
			return nil, errors.New("no document with that Id exists")
		}
		return nil, err
	}

	return employee, nil
}

func (e EmployeeService) SaveEmployee(employee *models.Employee) (*models.Employee, error) {
	employee.CreateDate = time.Now().UTC()
	employee.CreatedBy = "GolangUser-" + env.GetDefault("SITE", "default")
	employee.Version = 1
	employee.UUID = uuid.New().String()
	employee.UpdatedBy = "GolangUser-" + env.GetDefault("SITE", "default")
	employee.UpdatedDate = time.Now().UTC()

	res, err := e.employeeCollection.InsertOne(e.ctx, employee)

	if err != nil {
		return nil, err
	}

	opt := options.Index()
	opt.SetUnique(true)

	index := mongo.IndexModel{Keys: bson.M{"uuid": 1}, Options: opt}

	if _, err := e.employeeCollection.Indexes().CreateOne(e.ctx, index); err != nil {
		return nil, errors.New("could not create index for uuid")
	}

	var newEmployee *models.Employee

	query := bson.M{"_id": res.InsertedID}
	if err = e.employeeCollection.FindOne(e.ctx, query).Decode(&newEmployee); err != nil {
		return nil, err
	}

	dto := models.FromEmployeeToDTO(*employee)
	dto.State = models.CREATED

	if fail := e.dataGridRestClient.AddToCache(&dto); fail != nil {
		return nil, fail
	}

	return newEmployee, nil
}

func (e EmployeeService) UpdateEmployee(employeeId string, employee *models.Employee) (*models.Employee, error) {

	dbEmployee, err := e.GetEmployeeById(employeeId)
	if err != nil {
		return nil, err
	}

	exists, err := e.dataGridRestClient.KeyExistsInCache(dbEmployee.UUID)
	if err != nil {
		return nil, err
	}

	if exists {
		employeeFromCache, err := e.dataGridRestClient.GetEmployeeFromCache(dbEmployee.UUID)
		if err != nil {
			return nil, err
		}

		if dbEmployee.Version < employeeFromCache.Version {
			return nil, fmt.Errorf("The local version of employee %s is out of date. The most up-to-date version is %d, updated by %s on %s and local version is %d. Please update your local version", employee.UUID, employeeFromCache.Version, employeeFromCache.UpdatedBy, employeeFromCache.UpdatedDate, dbEmployee.Version)
		}
	}

	employee.Version = dbEmployee.Version + 1
	employee.UpdatedBy = "GolangUser-" + env.GetDefault("SITE", "default")
	employee.UpdatedDate = time.Now().UTC()
	employee.UUID = dbEmployee.UUID
	employee.CreatedBy = dbEmployee.CreatedBy
	employee.CreateDate = dbEmployee.CreateDate

	updatedEmployee, err := doEmployeeUpdate(employeeId, employee, err, e)
	if err != nil {
		return nil, err
	}

	dto := models.FromEmployeeToDTO(*employee)
	dto.State = models.UPDATED

	if fail := e.dataGridRestClient.AddToCache(&dto); fail != nil {
		return nil, fail
	}

	return updatedEmployee, nil
}

func doEmployeeUpdate(employeeId string, employee *models.Employee, err error, e EmployeeService) (*models.Employee, error) {
	doc, err := ToDoc(employee)
	if err != nil {
		return nil, err
	}

	obId, _ := primitive.ObjectIDFromHex(employeeId)
	query := bson.D{{Key: "_id", Value: obId}}
	update := bson.D{{Key: "$set", Value: doc}}
	res := e.employeeCollection.FindOneAndUpdate(e.ctx, query, update, options.FindOneAndUpdate().SetReturnDocument(1))

	var updatedEmployee *models.Employee

	if err := res.Decode(&updatedEmployee); err != nil {
		return nil, errors.New("no employee with that Id exists")
	}
	return updatedEmployee, nil
}

func (e EmployeeService) DeleteEmployee(employeeId string) error {

	obId, _ := primitive.ObjectIDFromHex(employeeId)
	query := bson.M{"_id": obId}

	res, err := e.employeeCollection.DeleteOne(e.ctx, query)
	if err != nil {
		return err
	}

	if res.DeletedCount == 0 {
		return errors.New("no document with that Id exists")
	}

	return nil

}

func (e EmployeeService) GetAllEmployeesKeysInCache() ([]string, error) {
	return e.dataGridRestClient.GetAllKeysFromCache()
}

func (e EmployeeService) UpdateEntityFromCache(employeeId string) error {

	dbEmployee, err := e.GetEmployeeById(employeeId)
	if err != nil {
		return err
	}

	employeeFromCache, err := e.dataGridRestClient.GetEmployeeFromCache(dbEmployee.UUID)
	if err != nil {
		return err
	}

	dbEmployee.UpdatedBy = employeeFromCache.UpdatedBy
	dbEmployee.UpdatedDate = employeeFromCache.UpdatedDate
	dbEmployee.Version = employeeFromCache.Version
	dbEmployee.Department = employeeFromCache.Department
	dbEmployee.Designation = employeeFromCache.Designation
	dbEmployee.FullName = employeeFromCache.FullName

	_, err = doEmployeeUpdate(employeeId, dbEmployee, err, e)
	if err != nil {
		return err
	}

	return nil
}

func (e EmployeeService) ImportEntityFromCache(uuid string) (string, error) {
	employeeFromCache, err := e.dataGridRestClient.GetEmployeeFromCache(uuid)
	if err != nil {
		return "", err
	}
	employee := models.FromDTOToEmployee(employeeFromCache)
	dbEmployee, err := e.GetEmployeeById(uuid)
	if err != nil {
		if !strings.Contains(err.Error(), "no document with") {
			employee.EmployeeId = dbEmployee.EmployeeId
			_, err := doEmployeeUpdate(employee.EmployeeId.String(), &employee, err, e)
			if err != nil {
				return "", err
			} else {
				return employee.EmployeeId.String(), nil
			}
		}

		if dbEmployee != nil {
			return "", err
		}
	}

	res, err := e.employeeCollection.InsertOne(e.ctx, employee)
	if err != nil {
		return "", err
	}

	opt := options.Index()
	opt.SetUnique(true)

	index := mongo.IndexModel{Keys: bson.M{"uuid": 1}, Options: opt}

	if _, err := e.employeeCollection.Indexes().CreateOne(e.ctx, index); err != nil {
		return "", errors.New("could not create index for uuid")
	}

	var newEmployee *models.Employee
	query := bson.M{"_id": res.InsertedID}
	if err = e.employeeCollection.FindOne(e.ctx, query).Decode(&newEmployee); err != nil {
		return "", err
	}

	return newEmployee.EmployeeId.Hex(), nil
}

func NewEmployeeService(employeeCollection *mongo.Collection, ctx context.Context) IEmployeeService {
	return &EmployeeService{employeeCollection, ctx, cache.NewDataGridRestClient()}
}

func ToDoc(v interface{}) (doc *bson.D, err error) {
	data, err := bson.Marshal(v)
	if err != nil {
		return
	}

	err = bson.Unmarshal(data, &doc)
	return
}
