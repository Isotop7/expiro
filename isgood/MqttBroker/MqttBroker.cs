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

using isgood.Models;
using isgood.Configuration;

namespace isgood.Mqtt;

public class MqttBroker
{
    private MqttServer mqttBroker;
    private Timer? bestBeforeTimeout;

    private List<Product> Products;

    public MqttBroker(MqttConfiguration mqttConfiguration) {
        Products = new();

        var options = new MqttServerOptionsBuilder()
            .WithDefaultEndpoint()
            .Build();

        var factory = new MqttFactory(new MqttBrokerLogger());
        mqttBroker = factory.CreateMqttServer(options);

        mqttBroker.InterceptingPublishAsync += e =>
        {
            if (e.ApplicationMessage.Topic.Equals(mqttConfiguration.TopicBarcode))
            {
                DispatchTopicBarcode(e, mqttConfiguration.BestBeforeTimeout);
            }
            else if (e.ApplicationMessage.Topic.Equals(mqttConfiguration.TopicBestBeforeSet))
            {
                DispatchTopicBestBeforeSet(e);
            }
            else if (e.ApplicationMessage.Topic.Equals(mqttConfiguration.TopicBarcodeRemove))
            {
                DispatchTopicBarcodeRemove(e);
            }

            return CompletedTask.Instance;
        };
    }

    public async Task Start(CancellationToken cancellationToken)
    {
        if (mqttBroker != null)
        {
            try {
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
            if (content != null && content.Equals(string.Empty) == false)
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

            if (product.Barcode != null && Regex.IsMatch(product.Barcode, AppConfiguration.BarcodeRegex, new(), TimeSpan.FromSeconds(30)) == false)
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
        {
            throw;
        }
    }

    private void DispatchTopicBestBeforeSet(InterceptingPublishEventArgs ipea)
    {
        Product product = new();
        try
        {
            string content = ipea.ApplicationMessage.ConvertPayloadToString().Trim() ?? string.Empty;
            if (content is not null && content.Equals(string.Empty) == false)
            {
                product = JsonConvert.DeserializeObject<Product>(content) ?? new();
            }
            else
            {
                throw new InvalidCastException("Payload was empty and could not be deserialized");
            }
        }
        catch
        {
            throw;
        }

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

    private void DispatchTopicBarcodeRemove(InterceptingPublishEventArgs ipea)
    {
        Product product = new();
        try
        {
            string content = ipea.ApplicationMessage.ConvertPayloadToString().Trim() ?? string.Empty;
            if (content is not null && content.Equals(string.Empty) == false)
            {
                product = JsonConvert.DeserializeObject<Product>(content) ?? new();
            }
            else
            {
                throw new InvalidCastException("Payload was empty and could not be deserialized");
            }
        }
        catch
        {
            throw;
        }

        Console.WriteLine($"+ embeddedBroker: Product with barcode '{product.Barcode}' published and enqueued for removal");

        Program.DatabaseQueue.Enqueue(new(Database.DatabaseQueueElementAction.DELETE, product));
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

                Program.APIQueue.Enqueue(matchedProduct);
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
