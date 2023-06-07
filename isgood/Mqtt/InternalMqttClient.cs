using System.Threading;
using System.Threading.Tasks;

using Newtonsoft.Json;
using MQTTnet;
using MQTTnet.Client;

using isgood.Models;
using isgood.Models.Converters.Product;
using isgood.Configuration;

namespace isgood.Mqtt;

public class InternalMqttClient
{
    private readonly MqttConfiguration _mqttConfiguration;

    public InternalMqttClient(MqttConfiguration mqttConfiguration)
    {
        _mqttConfiguration = mqttConfiguration;
    }

    public async Task HandleScannedAtPublish(string topic, Product product)
    {
        var mqttFactory = new MqttFactory();

        using (var mqttClient = mqttFactory.CreateMqttClient())
        {
            JsonSerializerSettings serializerSettings = new JsonSerializerSettings();
            serializerSettings.Converters.Add(new ProductScannedAtConverter());
            string payload = JsonConvert.SerializeObject(product, serializerSettings);

            var mqttClientOptions = new MqttClientOptionsBuilder()
                .WithTcpServer(_mqttConfiguration.BrokerURL, _mqttConfiguration.BrokerPort);
                
            if (_mqttConfiguration.BrokerAuthEnabled == true)
            {
                mqttClientOptions.WithCredentials(_mqttConfiguration.BrokerUsername, _mqttConfiguration.BrokerPassword);
            }

            await mqttClient.ConnectAsync(mqttClientOptions.Build(), CancellationToken.None);

            var applicationMessage = new MqttApplicationMessageBuilder()
                .WithTopic(topic)
                .WithPayload(payload)
                .Build();

            await mqttClient.PublishAsync(applicationMessage, CancellationToken.None);

            await mqttClient.DisconnectAsync();
        }
    }
}