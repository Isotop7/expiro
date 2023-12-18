package router

import (
	common "isgood/backend/handlers"
	v1 "isgood/backend/handlers/v1"

	"github.com/gin-gonic/gin"
	"gorm.io/gorm"
)

func SetupRouter(db *gorm.DB) *gin.Engine {
	r := gin.Default()

	r.Use(func(c *gin.Context) {
		c.Set("db", db)
		c.Next()
	})

	// Health routes
	r.GET("/health", common.GetHealth)
	// Product routes
	r.GET("/api/v1/products", v1.GetProducts)
	r.GET("/api/v1/products/:id", v1.GetProduct)
	r.POST("/api/v1/products", v1.CreateProduct)
	r.PATCH("/api/v1/products/:id", v1.UpdateProduct)
	r.DELETE("/api/v1/products/:id", v1.DeleteProduct)

	return r
}
