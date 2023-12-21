package controllers

import (
	"expiro/backend/models"
	"time"
)

type NotificationController struct {
	Configuration models.NotificationConfiguration
}

func Setup() (NotificationController, error) {

	return NotificationController{}, nil
}

func (nc NotificationController) Dispatch() {
	interval := time.Hour * time.Duration(nc.Configuration.Interval)
	go func() {
		for {
			// TODO: Send Mail
			time.Sleep(interval)
		}
	}()
}
