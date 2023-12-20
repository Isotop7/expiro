package v1

import (
	"expiro/backend/models"
	"fmt"
	"net/http"
	"strconv"

	"github.com/gin-gonic/gin"
	"gorm.io/gorm"
)

func GetProducts(c *gin.Context) {
	db, ok := c.MustGet("db").(*gorm.DB)
	if !ok {
		c.JSON(http.StatusInternalServerError, gin.H{"error": "Failed to get database from context"})
		return
	}

	var products []models.Product
	db.Find(&products)

	c.IndentedJSON(http.StatusOK, products)
}

func GetProduct(c *gin.Context) {
	id := c.Param("id")

	if _, err := strconv.Atoi(id); err != nil {
		c.IndentedJSON(http.StatusBadRequest, gin.H{"error": fmt.Sprintf("ID '%s' is invalid", id)})
		return
	}

	db, ok := c.MustGet("db").(*gorm.DB)
	if !ok {
		c.JSON(http.StatusInternalServerError, gin.H{"error": "Failed to get database from context"})
		return
	}

	var product models.Product
	getError := db.First(&product, id)

	if getError.Error != nil || product.ID <= 0 {
		c.IndentedJSON(http.StatusBadRequest, gin.H{"error": fmt.Sprintf("Product with id '%s' was not found", id)})
		return
	}

	c.IndentedJSON(http.StatusOK, product)
}

func CreateProduct(c *gin.Context) {
	db, ok := c.MustGet("db").(*gorm.DB)
	if !ok {
		c.JSON(http.StatusInternalServerError, gin.H{"error": "Failed to get database from context"})
		return
	}

	var product models.Product
	if err := c.ShouldBindJSON(&product); err != nil {
		c.JSON(http.StatusBadRequest, gin.H{"error": err.Error()})
		fmt.Println(err.Error())
		return
	}

	if product.Barcode == "" {
		c.JSON(http.StatusBadRequest, gin.H{"error": "barcode missing"})
		return
	}

	createResult := db.Create(&product)
	if createResult.Error != nil {
		c.JSON(http.StatusInternalServerError, gin.H{"error": createResult.Error.Error()})
		return
	}

	c.JSON(http.StatusCreated, product)
}

func UpdateProduct(c *gin.Context) {
	id := c.Param("id")

	if _, err := strconv.Atoi(id); err != nil {
		c.IndentedJSON(http.StatusBadRequest, gin.H{"error": fmt.Sprintf("ID '%s' is invalid", id)})
		return
	}

	db, ok := c.MustGet("db").(*gorm.DB)
	if !ok {
		c.JSON(http.StatusInternalServerError, gin.H{"error": "Failed to get database from context"})
		return
	}

	var product models.Product
	if err := c.ShouldBindJSON(&product); err != nil {
		c.JSON(http.StatusBadRequest, gin.H{"error": err.Error()})
		fmt.Println(err.Error())
		return
	}

	if product.Barcode == "" {
		c.JSON(http.StatusBadRequest, gin.H{"error": "barcode missing"})
		return
	}

	var dbProduct models.Product
	getError := db.First(&product, id)

	dbProduct.Barcode = product.Barcode
	dbProduct.ProductName = product.ProductName
	dbProduct.Categories = product.Categories
	dbProduct.Countries = product.Countries
	dbProduct.ImageURL = product.ImageURL
	dbProduct.BestBefore = product.BestBefore

	if getError.Error != nil || product.ID <= 0 {
		c.IndentedJSON(http.StatusBadRequest, gin.H{"error": fmt.Sprintf("Product with id '%s' was not found", id)})
		return
	}

	saveResult := db.Save(&product)
	if saveResult.Error != nil {
		c.JSON(http.StatusInternalServerError, gin.H{"error": saveResult.Error.Error()})
		return
	}

	c.JSON(http.StatusOK, product)
}

func DeleteProduct(c *gin.Context) {
	id := c.Param("id")

	if _, err := strconv.Atoi(id); err != nil {
		c.IndentedJSON(http.StatusBadRequest, gin.H{"error": fmt.Sprintf("ID '%s' is invalid", id)})
		return
	}

	db, ok := c.MustGet("db").(*gorm.DB)
	if !ok {
		c.JSON(http.StatusInternalServerError, gin.H{"error": "Failed to get database from context"})
		return
	}

	deleteResult := db.Delete(&models.Product{}, id)
	if deleteResult.Error != nil {
		c.JSON(http.StatusInternalServerError, gin.H{"error": deleteResult.Error.Error()})
		return
	}

	c.JSON(http.StatusOK, gin.H{"success": fmt.Sprintf("Product with ID '%s' was deleted", id)})
}
