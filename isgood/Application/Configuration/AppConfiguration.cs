using System;

namespace isgood.Configuration;

internal class AppConfiguration {
    
    public MqttConfiguration MqttConfiguration { get; set; }
    public ProductConfiguration ProductConfiguration { get; set; }
    public NotificationConfiguration NotificationConfiguration { get; set; }
    public WebUIConfiguration WebUIConfiguration { get; set; }
    public static readonly string OpenFoodFactsApiUrl = "https://world.openfoodfacts.org/api/v0/product";
    public static readonly string BarcodeRegex = "^\\d{13}$";

    public AppConfiguration() {
        MqttConfiguration = new();
        ProductConfiguration = new();
        NotificationConfiguration = new();
        WebUIConfiguration = new();
    }

    public bool IsValid() 
    {
        return  MqttConfiguration.IsValid() 
                && ProductConfiguration.IsValid()
                && NotificationConfiguration.IsValid()
                && WebUIConfiguration.IsValid();
    }
}