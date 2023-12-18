package main

import (
	"fmt"
	"isgood/backend/models"
	"isgood/backend/router"

	"github.com/spf13/viper"
	"gorm.io/driver/mysql"
	"gorm.io/gorm"
)

var db *gorm.DB

func main() {
	viper.SetConfigName("config")
	viper.SetConfigType("yaml")
	viper.AddConfigPath(".")
	viper.AutomaticEnv()

	configErr := viper.ReadInConfig()
	if configErr != nil {
		panic(configErr.Error())
	}

	dbHost := viper.GetString("database.host")
	dbPort := viper.GetInt("database.port")
	dbName := viper.GetString("database.name")
	dbUser := viper.GetString("database.user")
	dbPassword := viper.GetString("database.password")

	if dbHost == "" {
		panic("No database host specified")
	}
	if dbUser == "" {
		panic("No database user specified")
	}
	if dbPassword == "" {
		panic("No database password specified")
	}

	// Set default values if correctable invalid values were specified
	if dbName == "" {
		dbName = "isgood"
	}
	if dbPort <= 0 {
		dbPort = 3306
	}

	// Generate database URI
	databaseUri := fmt.Sprintf("%s:%s@tcp(%s:%d)/%s?charset=utf8mb4&parseTime=True&loc=Local",
		dbUser,
		dbPassword,
		dbHost,
		dbPort,
		dbName)

	var dbErr error
	db, dbErr = gorm.Open(mysql.Open(databaseUri), &gorm.Config{})

	// Check if database can be accessed
	if dbErr != nil {
		panic(dbErr.Error())
	}

	// Run migrations for database
	db.AutoMigrate(&models.Product{})

	// Call function to setup router and pass database interface
	r := router.SetupRouter(db)

	// Get server port or instead set default value
	serverPort := viper.GetInt("server.port")
	if serverPort <= 0 {
		serverPort = 5050
	}

	// Start server
	r.Run(fmt.Sprintf(":%d", serverPort))
}
