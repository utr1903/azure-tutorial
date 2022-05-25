using Azure.Messaging.ServiceBus;
using InputProcessor.EventHub;
using InputProcessor.InfluxDb;
using InputProcessor.Models;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace InputProcessor.ServiceBus
{
    public class ServiceBusHandler : IHostedService
    {
        private string _serviceBusConnectionString;
        private string _serviceBusQueueName;

        private EventHubHandler _eventHubHandler;
        private InfluxDbHandler _influxDbHandler;

        private ServiceBusClient _client;
        private ServiceBusProcessor _processor;

        public ServiceBusHandler()
        {
            // Create Event Hub producer client.
            CreateEventhHubProducer();

            // Create InfluxDB writer client.
            CreateInfluxDbClient();

            // Get Service Bus credentials.
            GetServiceBusCredentials();

            // Create Service Bus client.
            CreateServiceBusClient();

            // Create Service Bus processor.
            CreateServiceBusProcessor();
        }

        /// <summary>
        ///     Starts to process Service Bus Queue messages
        ///     and send them to Event Hub.
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns>
        ///     Task.
        /// </returns>
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

        /// <summary>
        ///     Stops processing Service Bus Queue messages gracefully.
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns>
        ///     Task.
        /// </returns>
        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await _processor.DisposeAsync();
            await _client.DisposeAsync();
        }

        /// <summary>
        ///     Creates Event Hub producer client.
        /// </summary>
        private void CreateEventhHubProducer()
        {
            _eventHubHandler = new EventHubHandler();
            _eventHubHandler.CreateProducerClient();
        }

        /// <summary>
        ///     Creates InfluxDB client.
        /// </summary>
        private void CreateInfluxDbClient()
        {
            _influxDbHandler = new InfluxDbHandler();
            _influxDbHandler.CreateClient();
        }

        /// <summary>
        ///     Gets Service Bus credentials from environment variables.
        /// </summary>
        private void GetServiceBusCredentials()
        {
            _serviceBusConnectionString = Environment.GetEnvironmentVariable("SERVICE_BUS_CONNECTION_STRING");
            _serviceBusQueueName = Environment.GetEnvironmentVariable("SERVICE_BUS_QUEUE_NAME");
        }

        /// <summary>
        ///     Creates Service Bus client.
        /// </summary>
        private void CreateServiceBusClient()
        {
            Console.WriteLine($"{DateTime.UtcNow}: Creating Service Bus connection...");

            _client = new ServiceBusClient(_serviceBusConnectionString);

            Console.WriteLine($"{DateTime.UtcNow}: -> Service Bus connection is created successfully.{Environment.NewLine}");
        }

        /// <summary>
        ///     Creates Service Bus processor.
        /// </summary>
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

        /// <summary>
        ///     Gets the message from the Service Bus Queue and
        ///     sends it to Event Hub.
        /// </summary>
        /// <param name="args"></param>
        /// <returns>
        ///     Task.
        /// </returns>
        private async Task MessageHandler(ProcessMessageEventArgs args)
        {
            try
            {
                string messageBody = args.Message.Body.ToString();

                Console.WriteLine($"Received: {messageBody}");

                var deviceMessage = ParseMessage(args.Message);

                // Send the message to Event Hub.
                await SendMessageToEventHub(messageBody);

                // Write the message to InfluxDB.
                await WriteMessageToInfluxDb(deviceMessage);

                // Acknowledge the message.
                await args.CompleteMessageAsync(args.Message);
            }
            catch (Exception e)
            {
                Console.WriteLine($"{DateTime.UtcNow}: Error occurred!");
                Console.WriteLine($"{DateTime.UtcNow}: -> {e.Message}.{Environment.NewLine}");
                Console.WriteLine($"{DateTime.UtcNow}: -> {e.InnerException}.{Environment.NewLine}");
            }
        }

        /// <summary>
        ///     Parse message into object.
        /// </summary>
        /// <param name="message"></param>
        /// <returns>
        ///     DeviceMessage.
        /// </returns>
        private DeviceMessage ParseMessage(ServiceBusReceivedMessage message)
            => JsonConvert.DeserializeObject<DeviceMessage>(
                message.Body.ToString());

        /// <summary>
        ///     Send message to Event Hub and acknowledge it.
        /// </summary>
        /// <param name="deviceMessage"></param>
        /// <returns>
        ///     Task.
        /// </returns>
        private async Task SendMessageToEventHub(string deviceMessage)
            => await _eventHubHandler.SendMessage(deviceMessage);

        /// <summary>
        ///     Writes message to Influx DB.
        /// </summary>
        /// <param name="deviceMessage"></param>
        /// <returns>
        ///     Task.
        /// </returns>
        private async Task WriteMessageToInfluxDb(DeviceMessage deviceMessage)
            => await _influxDbHandler.WriteMessage(deviceMessage);

        /// <summary>
        ///     Catches the error and logs it.
        /// </summary>
        /// <param name="args"></param>
        /// <returns>
        ///     Task.
        /// </returns>
        private Task ErrorHandler(ProcessErrorEventArgs args)
        {
            Console.WriteLine(args.Exception.ToString());
            return Task.CompletedTask;
        }
    }
}