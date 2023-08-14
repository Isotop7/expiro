using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

using MQTTnet;
using MQTTnet.Server;
using MQTTnet.Internal;
using Newtonsoft.Json;

using isgood.Database;
using isgood.Models;
using isgood.Configuration;

namespace isgood.Mqtt;

public class MqttBroker
{
    private readonly MqttServer mqttBroker;
    private MqttConfiguration _mqttConfiguration;
    private Timer? bestBeforeTimeout;
    private List<Product> Products;

    public MqttBroker(MqttConfiguration mqttConfiguration)
    {
        _mqttConfiguration = mqttConfiguration;
        Products = new();

        var options = new MqttServerOptionsBuilder()
            .WithDefaultEndpoint()
            .Build();

        var factory = new MqttFactory(new MqttBrokerLogger());
        mqttBroker = factory.CreateMqttServer(options);

        mqttBroker.InterceptingPublishAsync += e =>
        {
            if (e.ApplicationMessage.Topic.Equals(_mqttConfiguration.TopicBarcode))
            {
                DispatchTopicBarcode(e, _mqttConfiguration.BestBeforeTimeout);
            }
            else if (e.ApplicationMessage.Topic.Equals(_mqttConfiguration.TopicBestBeforeSet))
            {
                DispatchTopicBestBeforeSet(e);
            }
            else if (e.ApplicationMessage.Topic.Equals(_mqttConfiguration.TopicBarcodeRemove))
            {
                DispatchTopicBarcodeRemove(e);
            }
            else if (e.ApplicationMessage.Topic.Equals(_mqttConfiguration.TopicScannedAtGet))
            {
                DispatchTopicScannedAtGet(e);
            }
            else
            {
                DispatchUnknownTopic(e);
            }

            return CompletedTask.Instance;
        };
    }

    public async Task Start(CancellationToken cancellationToken)
    {
        if (mqttBroker == null)
        {
            throw new ObjectDisposedException("MQTT broker not set up");
        }

        try
        {
            await mqttBroker.StartAsync();

            while (!cancellationToken.IsCancellationRequested)
            {
                //TODO: Do we really need to do this here?
                await Task.Delay(TimeSpan.FromSeconds(10), cancellationToken);
            }

            // Stop the MQTT server
            await mqttBroker.StopAsync();
            mqttBroker.Dispose();
        }
        catch (TaskCanceledException)
        {
            Console.WriteLine($"+ embeddedBroker: Cancellation was requested");
            await mqttBroker.StopAsync();
            mqttBroker.Dispose();
        }
    }

    public async Task Stop()
    {
        if (mqttBroker != null)
        {
            await mqttBroker.StopAsync();
            mqttBroker.Dispose();
        }
    }

    private void DispatchTopicBarcode(InterceptingPublishEventArgs ipea, int timerTimeout)
    {
        Product? product = new();
        try
        {
            string content = ipea.ApplicationMessage.ConvertPayloadToString().Trim() ?? string.Empty;
            if (content != null && !string.IsNullOrEmpty(content))
            {
                product = JsonConvert.DeserializeObject<Product>(content);
            }
            else
            {
                throw new InvalidCastException("Payload was empty and could not be deserialized");
            }

            if (product == null)
            {
                throw new InvalidCastException("Product could not be deserialized");
            }

            if (product.Barcode != null && !Regex.IsMatch(product.Barcode, AppConfiguration.BarcodeRegex, new(), TimeSpan.FromSeconds(30)))
            {
                throw new InvalidOperationException("Published barcode does not match format");
            }

            int idx = Products.FindIndex(e => e.Barcode == product.Barcode);
            if (idx == -1)
            {
                Console.WriteLine($"+ embeddedBroker: New product with barcode '{product.Barcode}' published, starting timer with {timerTimeout} seconds timeout ...");

                bestBeforeTimeout = new Timer(
                    BestBeforeTimeoutTriggered,
                    product,
                    TimeSpan.FromSeconds(timerTimeout),
                    Timeout.InfiniteTimeSpan
                );

                Products.Add(product);
            }
        }
        catch
        { }
    }

    private void DispatchTopicBestBeforeSet(InterceptingPublishEventArgs ipea)
    {
        Product product = new();
        try
        {
            string content = ipea.ApplicationMessage.ConvertPayloadToString().Trim() ?? string.Empty;
            if (content is not null && !string.IsNullOrEmpty(content))
            {
                product = JsonConvert.DeserializeObject<Product>(content) ?? new();
            }
            else
            {
                throw new InvalidCastException("Payload was empty and could not be deserialized");
            }
        }
        catch
        { }

        Console.WriteLine($"+ embeddedBroker: Best before date for product with barcode '{product.Barcode}' published, trying to update date with value '{product.BestBefore}'");

        try
        {
            Product matchedProduct = Products.First(e => e.Barcode == product.Barcode);
            matchedProduct.BestBefore = product.BestBefore;
        }
        catch (InvalidOperationException)
        {
            Console.WriteLine($"+ embeddedBroker: Product with barcode '{product.Barcode}' cannot be updated, because it has to be published first");
        }
    }

    private static void DispatchTopicBarcodeRemove(InterceptingPublishEventArgs ipea)
    {
        Product product = new();
        try
        {
            string content = ipea.ApplicationMessage.ConvertPayloadToString().Trim() ?? string.Empty;
            if (content is not null && !string.IsNullOrEmpty(content))
            {
                product = JsonConvert.DeserializeObject<Product>(content) ?? new();
            }
            else
            {
                throw new InvalidCastException("Payload was empty and could not be deserialized");
            }
        }
        catch
        { }

        Console.WriteLine($"+ embeddedBroker: Product with barcode '{product.Barcode}' published and enqueued for removal");

        Program.DatabaseQueue.Enqueue(new(Database.DatabaseQueueElementAction.DELETE, product));
    }

    private async void DispatchTopicScannedAtGet(InterceptingPublishEventArgs ipea)
    {
        Product product = new();
        try
        {
            string content = ipea.ApplicationMessage.ConvertPayloadToString().Trim() ?? string.Empty;
            if (content is not null && !string.IsNullOrEmpty(content))
            {
                product = JsonConvert.DeserializeObject<Product>(content) ?? new();
            }
            else
            {
                throw new InvalidCastException("Payload was empty and could not be deserialized");
            }

            Console.WriteLine($"+ embeddedBroker: Got product with barcode '{product.Barcode}'. Publishing ScannedAt property.");

            AppDbContext appDbContext = new();
            Product? foundProduct = appDbContext.Product.FirstOrDefault(p => p.Barcode == product.Barcode);

            if (foundProduct == null)
            {
                throw new KeyNotFoundException($"Product with barcode {product.Barcode} was not found");
            }

            InternalMqttClient internalMqttClient = new(_mqttConfiguration);
            await internalMqttClient.HandleScannedAtPublish(_mqttConfiguration.TopicScannedAtPublish, foundProduct);
        }
        catch
        { }
    }

    private void DispatchUnknownTopic(InterceptingPublishEventArgs ipea)
    {
        Console.WriteLine($"+ embeddedBroker: Unknown topic '{ipea.ApplicationMessage.Topic}' or invalid content '{ipea.ApplicationMessage.ConvertPayloadToString().Trim()}'");
    }

    private void BestBeforeTimeoutTriggered(object? element)
    {
        if (element is not null)
        {
            Product product = (Product)element;
            if (product == null)
            {
                throw new InvalidCastException("Object could not be casted as product");
            }
            else
            {
                Console.WriteLine($"+ embeddedBroker: Timer for product '{product.Barcode}' finished");
                Product matchedProduct = Products.First(e => e.Barcode == product.Barcode);

                Program.ApiQueue.Enqueue(matchedProduct);
                Products.Remove(matchedProduct);
                Console.WriteLine($"+ embeddedBroker: Removed product with barcode '{product.Barcode}' from working list and queued it for API fulfillment");
            }
        }
        else
        {
            throw new InvalidOperationException("No value was specified in timer callback");
        }
    }
}
