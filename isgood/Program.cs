﻿namespace isgood;

using Microsoft.Extensions.Configuration;
using System;
using System.IO;

class Program
{
    private static AppConfiguration? _appConfiguration;
    static void Main(string[] args)
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

            if (_appConfiguration.MqttBrokerURL is null || _appConfiguration.MqttBrokerURL == string.Empty) 
            {
                throw new Exception("Configuration file is missing value 'MqttBrokerURL'");
            }
            else if (_appConfiguration.MqttBrokerPort is null || _appConfiguration.MqttBrokerPort < 0 ) 
            {
                throw new Exception("Configuration file is missing value 'MqttBrokerPort'");
            }
            else if (_appConfiguration.MqttBrokerAuthEnabled is not null) 
            {
                if (_appConfiguration.MqttBrokerAuthEnabled == true) 
                {
                    if (_appConfiguration.MqttBrokerUsername is null || _appConfiguration.MqttBrokerUsername == string.Empty)
                    {
                        throw new Exception("Configuration file is missing value 'MqttBrokerUsername'");
                    }
                    else if (_appConfiguration.MqttBrokerPassword is null || _appConfiguration.MqttBrokerPassword == string.Empty)
                    {
                        throw new Exception("Configuration file is missing value 'MqttBrokerPassword'");
                    }
                }
            }
        }
        catch (Exception ex) 
        {
            Console.WriteLine($"+ ERROR: {ex.Message}");
            System.Environment.Exit(1);
        }

        string brokerUrl = $"{_appConfiguration.MqttBrokerURL}:{_appConfiguration.MqttBrokerPort}";
        Console.WriteLine($"+ Connecting to mqtt broker '{brokerUrl}' ...");
    }
}