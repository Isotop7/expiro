namespace isgood;

internal class AppConfiguration {
    public string? MqttBrokerURL { get; set; }
    public int? MqttBrokerPort { get; set; }
    public string? MqttBrokerUsername { get; set; }
    public string? MqttBrokerPassword { get; set; }
    public string? MqttTopicBarcode { get; set; }
    public string? MqttTopicBestBeforeSet { get; set; }
    public int? MqttBestBeforeTimeout { get; set; }

    public AppConfiguration() {}
}