using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Hosting;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace InputProcessor.ServiceBus
{
    public class ServiceBusHandler : IHostedService
    {
        private string _serviceBusConnectionString;
        private string _serviceBusQueueName;

        private ServiceBusClient _client;
        private ServiceBusProcessor _processor;

        public ServiceBusHandler()
        {
            // Get Service Bus credentials.
            GetServiceBusCredentials();

            // Create Service Bus client.
            CreateServiceBusClient();

            // Create Service Bus processor.
            CreateServiceBusProcessor();
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            try
            {
                Console.WriteLine($"{DateTime.UtcNow}: Starting Service Bus processor...");

                await _processor.StartProcessingAsync();
            }
            catch (Exception e)
            {
                Console.WriteLine($"{DateTime.UtcNow}: Error occurred!");
                Console.WriteLine($"{DateTime.UtcNow}: -> {e.Message}.{Environment.NewLine}");
                Console.WriteLine($"{DateTime.UtcNow}: -> {e.InnerException}.{Environment.NewLine}");

                await _processor.DisposeAsync();
                await _client.DisposeAsync();
            }
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await _processor.DisposeAsync();
            await _client.DisposeAsync();
        }

        private void GetServiceBusCredentials()
        {
            _serviceBusConnectionString = Environment.GetEnvironmentVariable("SERVICE_BUS_CONNECTION_STRING");
            _serviceBusQueueName = Environment.GetEnvironmentVariable("SERVICE_BUS_QUEUE_NAME");
        }

        private void CreateServiceBusClient()
        {
            Console.WriteLine($"{DateTime.UtcNow}: Creating Service Bus connection...");

            _client = new ServiceBusClient(_serviceBusConnectionString);

            Console.WriteLine($"{DateTime.UtcNow}: -> Service Bus connection is created successfully.{Environment.NewLine}");
        }

        private void CreateServiceBusProcessor()
        {
            Console.WriteLine($"{DateTime.UtcNow}: Creating Service Bus processor...");

            var serviceBusProcessorOptions = new ServiceBusProcessorOptions
            {
                ReceiveMode = ServiceBusReceiveMode.PeekLock
            };

            _processor = _client.CreateProcessor(_serviceBusQueueName, serviceBusProcessorOptions);

            // add handler to process messages
            _processor.ProcessMessageAsync += MessageHandler;

            // add handler to process any errors
            _processor.ProcessErrorAsync += ErrorHandler;

            Console.WriteLine($"{DateTime.UtcNow}: -> Service Bus processor is created successfully.{Environment.NewLine}");
        }

        // handle received messages
        private async Task MessageHandler(ProcessMessageEventArgs args)
        {
            string body = args.Message.Body.ToString();
            Console.WriteLine($"Received: {body}");

            // complete the message. message is deleted from the queue. 
            await args.CompleteMessageAsync(args.Message);
        }

        // handle any errors when receiving messages
        private Task ErrorHandler(ProcessErrorEventArgs args)
        {
            Console.WriteLine(args.Exception.ToString());
            return Task.CompletedTask;
        }
    }
}