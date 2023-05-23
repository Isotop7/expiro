using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Concurrent;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;

using isgood.Models;
using isgood.Configuration;
using isgood.Mqtt;
using isgood.OpenFoodFactsAPI;
using isgood.Database;
using isgood.Notification;

namespace isgood;

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
        Console.WriteLine($"+ Starting isgood with config from directory {Directory.GetCurrentDirectory()}");

        try 
        {
            var builder = new ConfigurationBuilder();
			builder
				.SetBasePath(Directory.GetCurrentDirectory())
				.AddJsonFile("isgood.json", false);

			IConfigurationRoot configuration = builder.Build();
			_appConfiguration = new();
            configuration.Bind(_appConfiguration);
            if (_appConfiguration.IsValid() == false)
            {
                throw new ArgumentException("AppConfiguration is invalid");
            }
        }
        catch (Exception ex) 
        {
            Console.WriteLine($"+ ERROR: {ex.Message}");
            System.Environment.Exit(1);
        }

        if (_appConfiguration.WebUIConfiguration.Enabled == true)
        {
            Console.WriteLine("+ isgood: Starting WebUI");
            Task webUITask = Task.Run(() => StartWebUIAsync());
            Console.WriteLine("+ isgood: WebUI started");
        }

        if (_appConfiguration.NotificationConfiguration.Enabled == true)
        {
            Console.WriteLine("+ isgood: Starting notification service");
            Task embeddedBrokerTask = Task.Run(() => StartNotificationServiceAsync());
            Console.WriteLine("+ isgood: notification service started");
        }

        if (_appConfiguration.MqttConfiguration.UseEmbedded == true)
        {
            Console.WriteLine("+ isgood: Starting embedded mqtt broker");
            Task embeddedBrokerTask = Task.Run(() => StartEmbeddedBrokerAsync());
            Console.WriteLine("+ isgood: embeddedBroker started");

            Console.WriteLine("+ isgood: Starting DatabaseWorkerService");
            Task databaseWorkerServiceTask = Task.Run(() => StartDatabaseWorkerServiceAsync());
            Console.WriteLine("+ isgood: DatabaseWorkerService started");

            Console.WriteLine("+ isgood: Starting APIWorkerService");
            Task apiWorkerServiceTask = Task.Run(() => StartAPIWorkerServiceAsync());
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

    private static async Task StartAPIWorkerServiceAsync()
    {
        if (_appConfiguration != null)
        {
            APIWorkerService apiWorkerService = new APIWorkerService(_appConfiguration);
            await apiWorkerService.StartAsync(_cancellationTokenSource.Token);
        }
    }

    private static async Task StartDatabaseWorkerServiceAsync()
    {
        DatabaseWorkerService databaseWorkerService = new DatabaseWorkerService();
        await databaseWorkerService.StartAsync(_cancellationTokenSource.Token);
    }

    private static async Task StartNotificationServiceAsync()
    {
        if (_appConfiguration != null)
        {
            NotificationService notificationService = new NotificationService(_appConfiguration.NotificationConfiguration, _appConfiguration.ProductConfiguration.BestBeforeWarnDelta ?? 2);
            await notificationService.StartAsync(_cancellationTokenSource.Token);
        }
        else
        {
            throw new ArgumentException("AppConfiguration missing");
        }
    }

    private static async Task StartWebUIAsync()
    {
        WebApplicationBuilder builder = WebApplication.CreateBuilder();
        
        // Add services to the container.
        builder.Services.AddRazorPages();
        builder.Services.AddHealthChecks();

        builder.Services.AddDbContext<AppDbContext>();
        if (_appConfiguration != null)
        {
            builder.Services.AddSingleton<ProductConfiguration>(_appConfiguration.ProductConfiguration);
        }

        WebApplication app = builder.Build();

        // Configure the HTTP request pipeline.
        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Error");
            // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            app.UseHsts();
        }

        app.UseHttpsRedirection();
        app.UseStaticFiles();

        app.UseRouting();

        app.UseAuthorization();

        app.MapHealthChecks("/healthz");
        app.MapRazorPages();

        await app.RunAsync();
    }
}