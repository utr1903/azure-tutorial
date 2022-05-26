using Azure.Messaging.ServiceBus;
using InputProcessor.Commons;
using InputProcessor.EventHub;
using InputProcessor.InfluxDb;
using InputProcessor.Models;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace InputProcessor.ServiceBus
{
    public class ServiceBusHandler : IHostedService
    {
        private readonly ILogger _logger;

        private string _serviceBusConnectionString;
        private string _serviceBusQueueName;

        private EventHubHandler _eventHubHandler;
        private InfluxDbHandler _influxDbHandler;

        private ServiceBusClient _client;
        private ServiceBusProcessor _processor;

        public ServiceBusHandler(ILogger<ServiceBusHandler> logger)
        {
            // Set logger.
            _logger = logger;

            // Create Event Hub producer client.
            CreateEventhHubProducer();

            // Create InfluxDB writer client.
            CreateInfluxDbClient();

            // Get Service Bus credentials.
            GetServiceBusCredentials();

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
                LogStartingServiceBusProcessor();

                await _processor.StartProcessingAsync();
            }
            catch (Exception e)
            {
                LogUnexpectedErrorOccured(e);

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
            _eventHubHandler = new EventHubHandler(_logger);
            _eventHubHandler.CreateProducerClient();
        }

        /// <summary>
        ///     Creates InfluxDB client.
        /// </summary>
        private void CreateInfluxDbClient()
        {
            _influxDbHandler = new InfluxDbHandler(_logger);
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
        ///     Creates Service Bus processor.
        /// </summary>
        private void CreateServiceBusProcessor()
        {
            LogCreatingServiceBusProcessor();

            _client = new ServiceBusClient(_serviceBusConnectionString);

            var serviceBusProcessorOptions = new ServiceBusProcessorOptions
            {
                ReceiveMode = ServiceBusReceiveMode.PeekLock
            };

            _processor = _client.CreateProcessor(_serviceBusQueueName, serviceBusProcessorOptions);

            // add handler to process messages
            _processor.ProcessMessageAsync += MessageHandler;

            // add handler to process any errors
            _processor.ProcessErrorAsync += ErrorHandler;

            LogServiceBusProcessorCreated();
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
                var deviceMessage = ParseMessage(args.Message);

                // Send the message to Event Hub.
                await SendMessageToEventHub(deviceMessage);

                // Write the message to InfluxDB.
                await WriteMessageToInfluxDb(deviceMessage);

                // Acknowledge the message.
                await args.CompleteMessageAsync(args.Message);
            }
            catch (Exception e)
            {
                LogUnexpectedErrorOccured(e);
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
        {
            LogParsingServiceBusMessage();

            var deviceMessage = JsonConvert.DeserializeObject<DeviceMessage>(
                message.Body.ToString());

            LogServiceBusMessageParsed();

            return deviceMessage;
        }

        /// <summary>
        ///     Send message to Event Hub and acknowledge it.
        /// </summary>
        /// <param name="deviceMessage">
        ///     Device message object.
        /// </param>
        /// <returns>
        ///     Task.
        /// </returns>
        private async Task SendMessageToEventHub(DeviceMessage deviceMessage)
            => await _eventHubHandler.SendMessage(deviceMessage);

        /// <summary>
        ///     Writes message to Influx DB.
        /// </summary>
        /// <param name="deviceMessage">
        ///     Device message object.
        /// </param>
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
            LogServiceBusErrorOccured(args.Exception);
            return Task.CompletedTask;
        }

        /// <summary>
        ///     Log creating Service Bus processor.
        /// </summary>
        private void LogCreatingServiceBusProcessor()
        {
            CustomLogger.Log(
                _logger,
                LogLevel.Information,
                nameof(ServiceBusHandler),
                nameof(CreateServiceBusProcessor),
                "Creating Service Bus processor..."
            );
        }

        /// <summary>
        ///     Log Service Bus processor created.
        /// </summary>
        private void LogServiceBusProcessorCreated()
        {
            CustomLogger.Log(
                _logger,
                LogLevel.Information,
                nameof(ServiceBusHandler),
                nameof(CreateServiceBusProcessor),
                "Service Bus processor is created successfully."
            );
        }

        /// <summary>
        ///     Log starting Service Bus processor.
        /// </summary>
        private void LogStartingServiceBusProcessor()
        {
            CustomLogger.Log(
                _logger,
                LogLevel.Information,
                nameof(ServiceBusHandler),
                nameof(ParseMessage),
                "Starting Service Bus processor..."
            );
        }

        /// <summary>
        ///     Log parsing Service Bus message.
        /// </summary>
        private void LogParsingServiceBusMessage()
        {
            CustomLogger.Log(
                _logger,
                LogLevel.Information,
                nameof(ServiceBusHandler),
                nameof(ParseMessage),
                "Parsing Service Bus message..."
            );
        }

        /// <summary>
        ///     Log Service Bus message parsed.
        /// </summary>
        private void LogServiceBusMessageParsed()
        {
            CustomLogger.Log(
                _logger,
                LogLevel.Information,
                nameof(ServiceBusHandler),
                nameof(ParseMessage),
                "Service Bus message parsed..."
            );
        }

        /// <summary>
        ///     Log unexpected error occurred.
        /// </summary>
        private void LogUnexpectedErrorOccured(Exception e)
        {
            CustomLogger.Log(
                _logger,
                LogLevel.Error,
                nameof(ServiceBusHandler),
                nameof(MessageHandler),
                $"Unexpected error! Message: {e.Message}. InnerException:{e.InnerException}."
            );
        }

        /// <summary>
        ///     Log Service Bus error occurred.
        /// </summary>
        private void LogServiceBusErrorOccured(Exception e)
        {
            CustomLogger.Log(
                _logger,
                LogLevel.Error,
                nameof(ServiceBusHandler),
                nameof(ErrorHandler),
                $"Service Bus error! Message: {e.Message}. InnerException:{e.InnerException}."
            );
        }
    }
}