package database

import (
	"context"
	"fmt"
	"github.com/caitlinelfring/go-env-default"
	"go.mongodb.org/mongo-driver/mongo"
	"go.mongodb.org/mongo-driver/mongo/options"
	"log"
	"strconv"
	"time"
)

func ConnectDB() *mongo.Client {
	MongoUrl := "mongodb://" + env.GetDefault("MONGO_USER", "root") + ":" + env.GetDefault("MONGO_PASSWD", "password") + "@" + env.GetDefault("MONGO_HOST", "127.0.0.1") + ":" + strconv.Itoa(env.GetIntDefault("MONGO_PORT", 27017))
	client, err := mongo.NewClient(options.Client().ApplyURI(MongoUrl))

	if err != nil {
		log.Fatal(err)
	}

	ctx, cancel := context.WithTimeout(context.Background(), 10*time.Second)
	err = client.Connect(ctx)
	defer cancel()

	if err != nil {
		log.Fatal(err)
	}

	fmt.Println("Connecting to mongoDB")
	return client
}

func GetCollection() *mongo.Collection {
	client := ConnectDB()
	collection := client.Database(env.GetDefault("MONGO_DB", "employeedb")).Collection(env.GetDefault("MONGO_COLLECTION", "employees"))
	return collection
}
