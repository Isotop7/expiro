using System;

namespace isgood.Configuration;

internal class AppConfiguration {
    
    public MqttConfiguration MqttConfiguration { get; set; }
    public static readonly string OpenFoodFactsApiUrl = "https://world.openfoodfacts.org/api/v0/product";
    public static readonly string BarcodeRegex = "^\\d{13}$";

    public AppConfiguration() {
        MqttConfiguration = new();
    }

    public void Validate() 
    {
        // Check for mqtt mode
        if (MqttConfiguration.UseEmbedded is null)
        {
            throw new Exception("Configuration file is missing property 'UseEmbedded' for MqttConfiguration");
        }

        if (MqttConfiguration.BrokerURL is null || MqttConfiguration.BrokerURL == string.Empty) 
        {
            throw new Exception("Configuration file is missing value 'BrokerURL'");
        }
        else if (MqttConfiguration.BrokerPort is null || MqttConfiguration.BrokerPort < 0 ) 
        {
            throw new Exception("Configuration file is missing value 'BrokerPort'");
        }
        else if (MqttConfiguration.BrokerAuthEnabled is not null) 
        {
            if (MqttConfiguration.BrokerAuthEnabled == true) 
            {
                // If auth is enabled, we need credentials
                if (MqttConfiguration.BrokerUsername is null || MqttConfiguration.BrokerUsername == string.Empty)
                {
                    throw new Exception("Configuration file is missing value 'BrokerUsername'");
                }
                else if (MqttConfiguration.BrokerPassword is null || MqttConfiguration.BrokerPassword == string.Empty)
                {
                    throw new Exception("Configuration file is missing value 'BrokerPassword'");
                }
            }
        }
    }
}