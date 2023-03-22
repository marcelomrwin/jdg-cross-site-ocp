package model

import (
	"go.mongodb.org/mongo-driver/bson/primitive"
)

type Employee struct {
	Id primitive.ObjectID `json:"id,omitempty" bson:"_id,omitempty"`
}
