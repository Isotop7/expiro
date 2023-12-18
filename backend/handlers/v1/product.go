package v1

import (
	"fmt"
	"isgood/backend/models"
	"net/http"

	"github.com/gin-gonic/gin"
	"gorm.io/gorm"
)

/*
 *	GET		'/api/v1/products'
 */
func GetProducts(c *gin.Context) {
	db, ok := c.MustGet("db").(*gorm.DB)
	if !ok {
		c.JSON(http.StatusInternalServerError, gin.H{"error": "Failed to get database from context"})
		return
	}

	var products []models.Product
	db.Find(&products)

	c.JSON(http.StatusOK, products)
}

/*
 *	GET		'/api/v1/products/:id'
 */
func GetProduct(c *gin.Context) {}

/*
 *	POST	'/api/v1/products'
 */
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

/*
 *	PATCH	'/api/v1/products/:id'
 */
func UpdateProduct(c *gin.Context) {}

/*
 *	DELETE	'/api/v1/products/:id'
 */
func DeleteProduct(c *gin.Context) {}
