using System;

namespace isgood.Configuration;

public class MqttConfiguration
{
    public bool? UseEmbedded { get; set; }
    public string? BrokerURL { get; set; }
    public int? BrokerPort { get; set; }
    public bool? BrokerAuthEnabled { get; set; }
    public string? BrokerUsername { get; set; }
    public string? BrokerPassword { get; set; }
    public string TopicBarcode { get; set; }
    public string TopicBarcodeRemove { get; set; }
    public string TopicBestBeforeSet { get; set; }
    public string TopicScannedAtGet { get; set; }
    public string TopicScannedAtPublish { get; set; }
    public int BestBeforeTimeout { get; set; }

    public MqttConfiguration()
    {
        TopicBarcode = "product/barcode";
        TopicBestBeforeSet = "product/best_before/set";
        TopicBarcodeRemove = "product/barcode/remove";
        TopicScannedAtGet = "product/scanned_at/get";
        TopicScannedAtPublish = "product/scanned_at/publish";
        BestBeforeTimeout = 15;
    }

    public bool IsValid()
    {
        // Check for mqtt mode
        if (UseEmbedded is null)
        {
            throw new ArgumentException("Configuration file is missing property 'UseEmbedded' for MqttConfiguration");
        }

        if (UseEmbedded == true)
        {
            BrokerURL = "127.0.0.1";
        }

        if (UseEmbedded == false && (BrokerURL is null || BrokerURL == string.Empty) ) 
        {
            throw new ArgumentException("Configuration file is missing value 'BrokerURL'");
        }
        
        if (BrokerPort is null || BrokerPort < 0 ) 
        {
            throw new ArgumentException("Configuration file is missing value 'BrokerPort'");
        }
        
        if (BrokerAuthEnabled is not null && BrokerAuthEnabled == true) 
        {
            // If auth is enabled, we need credentials
            if (BrokerUsername is null || BrokerUsername == string.Empty)
            {
                throw new ArgumentException("Configuration file is missing value 'BrokerUsername'");
            }
            else if (BrokerPassword is null || BrokerPassword == string.Empty)
            {
                throw new ArgumentException("Configuration file is missing value 'BrokerPassword'");
            }
        }

        return true;
    }
}