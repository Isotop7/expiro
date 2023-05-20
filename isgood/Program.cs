namespace isgood;

using System;
using System.IO;
using System.Threading.Tasks;
using System.Collections.Concurrent;

using Microsoft.Extensions.Configuration;

public class Program
{
    private static AppConfiguration? _appConfiguration;
    public static ConcurrentQueue<Product> DatabaseQueue
    {
        get;
    } = new ConcurrentQueue<Product>();
    public static ConcurrentQueue<Product> APIQueue
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
            embeddedBroker.Start();
            Console.WriteLine("+ isgood: embeddedBroker started");

            DatabaseWorkerService databaseWorkerService = new();
            databaseWorkerService.Start();
            Console.WriteLine("+ isgood: DatabaseWorkerService started");

            APIWorkerService apiWorkerService = new(_appConfiguration);
            apiWorkerService.Start();
            Console.WriteLine("+ isgood: APIWorkerService started");
        }
        else
        {
            string brokerUrl = $"{_appConfiguration.MqttConfiguration.BrokerURL}:{_appConfiguration.MqttConfiguration.BrokerPort}";
            Console.WriteLine($"+ Connecting to mqtt broker '{brokerUrl}' ...");
        }
    }
}