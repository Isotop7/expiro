namespace isgood;

using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Concurrent;

using Microsoft.Extensions.Configuration;

using isgood.Models;
using isgood.Configuration;
using isgood.Mqtt;
using isgood.OpenFoodFactsAPI;
using isgood.Database;

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
    private static CancellationTokenSource _cancellationTokenSource { get; } = new CancellationTokenSource();

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
            Task embeddedBrokerTask = Task.Run(() => StartEmbeddedBrokerAsync());
            Console.WriteLine("+ isgood: embeddedBroker started");

            Console.WriteLine("+ isgood: Starting DatabaseWorkerService");
            Task databaseWorkerServiceTask = Task.Run(() => StartDatabaseWorkerService());
            Console.WriteLine("+ isgood: DatabaseWorkerService started");

            Console.WriteLine("+ isgood: Starting APIWorkerServoce");
            Task apiWorkerServiceTask = Task.Run(() => StartAPIWorkerService());
            Console.WriteLine("+ isgood: APIWorkerService started");

            Console.WriteLine("+ isgood: Press CTRL+C to stop all tasks");
            Console.CancelKeyPress += (sender, eventArgs) =>
            {
                eventArgs.Cancel = true;
                _cancellationTokenSource.Cancel();
            };

            try
            {
                // Wait for the worker services to complete or cancellation signal
                await Task.WhenAny(embeddedBrokerTask, databaseWorkerServiceTask, apiWorkerServiceTask, Task.Delay(Timeout.Infinite, _cancellationTokenSource.Token));
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine("+ isgood: Cancellation was requestes. Shutting down services");
            }

            _cancellationTokenSource.Cancel();
            await Task.WhenAll(embeddedBrokerTask, apiWorkerServiceTask, databaseWorkerServiceTask);

        }
        else
        {
            string brokerUrl = $"{_appConfiguration.MqttConfiguration.BrokerURL}:{_appConfiguration.MqttConfiguration.BrokerPort}";
            Console.WriteLine($"+ Connecting to mqtt broker '{brokerUrl}' ...");
        }
    }

    private static async Task StartEmbeddedBrokerAsync()
    {
        if (_appConfiguration != null)
        {
            MqttBroker embeddedBroker = new MqttBroker(_appConfiguration.MqttConfiguration);
            await embeddedBroker.Start(_cancellationTokenSource.Token);
        }
    }

    private static async Task StartAPIWorkerService()
        {
            if (_appConfiguration != null)
            {
                APIWorkerService apiWorkerService = new APIWorkerService(_appConfiguration);
                await apiWorkerService.StartAsync(_cancellationTokenSource.Token);
            }
        }

        private static async Task StartDatabaseWorkerService()
        {
            DatabaseWorkerService databaseWorkerService = new DatabaseWorkerService();
            await databaseWorkerService.StartAsync(_cancellationTokenSource.Token);
        }
}