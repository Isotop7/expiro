namespace isgood;

using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Collections.Concurrent;

public class Program
{
    private static AppConfiguration? _appConfiguration;
    public static ConcurrentQueue<Product> DatabaseQueue
    {
        get;
    } = new ConcurrentQueue<Product>();

    static async Task Main(string[] args)
    {
        Console.WriteLine(@"
            ┬┌─┐┌─┐┌─┐┌─┐┌┬┐
            │└─┐│ ┬│ ││ │ ││
            ┴└─┘└─┘└─┘└─┘─┴┘
        ");
        Console.WriteLine("+ Starting isgood");

        try 
        {
            var builder = new ConfigurationBuilder();
			builder
				.SetBasePath(Directory.GetCurrentDirectory())
				.AddJsonFile("isgood.json", false);

			IConfigurationRoot configuration = builder.Build();
			_appConfiguration = new();
            configuration.Bind(_appConfiguration);
            _appConfiguration.Validate();
        }
        catch (Exception ex) 
        {
            Console.WriteLine($"+ ERROR: {ex.Message}");
            System.Environment.Exit(1);
        }

        if (_appConfiguration.MqttConfiguration.UseEmbedded == true)
        {
            Console.WriteLine("+ Starting embedded mqtt broker");
            MqttBroker embeddedBroker = new(_appConfiguration.MqttConfiguration);
            await embeddedBroker.Start();

            InsertionWorkerService insertionWorkerService = new();
            await insertionWorkerService.Start();

            // TODO: Read barcode from message
            // TODO: Query API with barcode
            // TODO: Saturate package with informations
            // TODO: Listen on CTRL+C to quit

            Console.WriteLine("+ embeddedBroker started. Press any key to stop ...");
            Console.ReadLine();

            await embeddedBroker.Stop();
        }
        else
        {
            string brokerUrl = $"{_appConfiguration.MqttConfiguration.BrokerURL}:{_appConfiguration.MqttConfiguration.BrokerPort}";
            Console.WriteLine($"+ Connecting to mqtt broker '{brokerUrl}' ...");
        }
    }
}