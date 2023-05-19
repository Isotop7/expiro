namespace isgood;

internal class MqttConfiguration
{
    public bool? UseEmbedded { get; set; }
    public string? BrokerURL { get; set; }
    public int? BrokerPort { get; set; }
    public bool? BrokerAuthEnabled { get; set; }
    public string? BrokerUsername { get; set; }
    public string? BrokerPassword { get; set; }
    public string TopicBarcode { get; set; }
    public string TopicBestBeforeSet { get; set; }
    public int BestBeforeTimeout { get; set; }

    public MqttConfiguration ()
    {
        TopicBarcode = "product/barcode";
        TopicBestBeforeSet = "product/best_before/set";
        BestBeforeTimeout = 15;
    }
}