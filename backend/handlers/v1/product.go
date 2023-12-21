package v1

import (
	"expiro/backend/controllers"
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

	c.JSON(http.StatusOK, products)
}

func GetProduct(c *gin.Context) {
	id := c.Param("id")

	if _, err := strconv.Atoi(id); err != nil {
		c.JSON(http.StatusBadRequest, gin.H{"error": fmt.Sprintf("ID '%s' is invalid", id)})
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
		c.JSON(http.StatusBadRequest, gin.H{"error": fmt.Sprintf("Product with id '%s' was not found", id)})
		return
	}

	c.JSON(http.StatusOK, product)
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

	cntrl, ok := c.MustGet("cntrl").(controllers.OpenFoodFactsAPIController)
	if !ok {
		c.JSON(http.StatusInternalServerError, gin.H{"error": "Failed to get controller from context"})
		return
	}
	var apiProduct models.Product
	apiProduct, err := cntrl.GetDataset(db, product.Barcode)
	if err == nil {
		product = apiProduct
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
		c.JSON(http.StatusBadRequest, gin.H{"error": fmt.Sprintf("ID '%s' is invalid", id)})
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
	getError := db.First(&dbProduct, id)

	dbProduct.Barcode = product.Barcode
	dbProduct.ProductName = product.ProductName
	dbProduct.Categories = product.Categories
	dbProduct.Countries = product.Countries
	dbProduct.ImageURL = product.ImageURL
	dbProduct.ExpireAt = product.ExpireAt

	if getError.Error != nil || product.ID <= 0 {
		c.JSON(http.StatusBadRequest, gin.H{"error": fmt.Sprintf("Product with id '%s' was not found", id)})
		return
	}

	saveResult := db.Save(&dbProduct)
	if saveResult.Error != nil {
		c.JSON(http.StatusInternalServerError, gin.H{"error": saveResult.Error.Error()})
		return
	}

	c.JSON(http.StatusOK, product)
}

func DeleteProduct(c *gin.Context) {
	id := c.Param("id")

	if _, err := strconv.Atoi(id); err != nil {
		c.JSON(http.StatusBadRequest, gin.H{"error": fmt.Sprintf("ID '%s' is invalid", id)})
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

func SetExpireAt(c *gin.Context) {
	id := c.Param("id")

	if _, err := strconv.Atoi(id); err != nil {
		c.JSON(http.StatusBadRequest, gin.H{"error": fmt.Sprintf("ID '%s' is invalid", id)})
		return
	}

	db, ok := c.MustGet("db").(*gorm.DB)
	if !ok {
		c.JSON(http.StatusInternalServerError, gin.H{"error": "Failed to get database from context"})
		return
	}

	var expireAt models.Timestamp
	if err := c.ShouldBindJSON(&expireAt); err != nil {
		c.JSON(http.StatusBadRequest, gin.H{"error": err.Error()})
		fmt.Println(err.Error())
		return
	}

	var dbProduct models.Product
	getError := db.First(&dbProduct, id)

	if getError.Error != nil {
		c.JSON(http.StatusBadRequest, gin.H{"error": fmt.Sprintf("Product with id '%s' was not found", id)})
		return
	}

	dbProduct.ExpireAt = expireAt.Timestamp

	saveResult := db.Save(&dbProduct)
	if saveResult.Error != nil {
		c.JSON(http.StatusInternalServerError, gin.H{"error": saveResult.Error.Error()})
		return
	}

	expireDTO := models.ProductDTOExpire{
		Barcode:  dbProduct.Barcode,
		ExpireAt: dbProduct.ExpireAt,
	}

	c.JSON(http.StatusOK, expireDTO)
}
