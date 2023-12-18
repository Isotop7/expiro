package models

import (
	"time"

	"gorm.io/gorm"
)

type Product struct {
	gorm.Model
	Barcode     string    `json:"barcode"`
	ProductName string    `json:"productName"`
	Categories  string    `json:"categories"`
	Countries   string    `json:"countries"`
	ImageURL    string    `json:"imageUrl"`
	BestBefore  time.Time `json:"bestBefore"`
	ScannedAt   time.Time `json:"scannedAt"`
	NotifiedAt  time.Time `json:"notifiedAt"`
}
