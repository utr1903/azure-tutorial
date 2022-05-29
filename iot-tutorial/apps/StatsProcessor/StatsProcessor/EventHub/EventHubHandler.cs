using System;
using System.Threading;
using System.Threading.Tasks;
using Azure.Messaging.EventHubs;
using Azure.Messaging.EventHubs.Processor;
using Azure.Storage.Blobs;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using StatsProcessor.Commons;
using StatsProcessor.Models;

namespace StatsProcessor.EventHub
{
    public class EventHubHandler : IHostedService
    {
        private readonly ILogger _logger;

        private const string STORAGE_ACCOUNT_CONNECTION_STRING = "STORAGE_ACCOUNT_CONNECTION_STRING";
        private const string BLOB_CONTAINER_NAME = "BLOB_CONTAINER_NAME";

        private const string EVENT_HUB_CONNECTION_STRING = "EVENT_HUB_CONNECTION_STRING";
        private const string EVENT_HUB_NAME = "EVENT_HUB_NAME";

        private EventProcessorClient _processor;

        public EventHubHandler(ILogger<EventHubHandler> logger)
        {
            // Set logger.
            _logger = logger;

            // Create Event Hub processor client.
            CreateProcessorClient();
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            try
            {
                LogStartingEventHubProcessor();

                await _processor.StartProcessingAsync();
            }
            catch (Exception e)
            {
                LogUnexpectedErrorOccured(e);
            }
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            Task processingTask = Task.Run(() => {
                LogStoppingEventHubProcessor();
            });

            await Task.WhenAll(processingTask);
        }

        /// <summary>
        ///     Creates Event Hub processor client.
        /// </summary>
        public void CreateProcessorClient()
        {
            LogCreatingEventHubProcessorClient();

            var storageConnectionString = Environment.GetEnvironmentVariable(STORAGE_ACCOUNT_CONNECTION_STRING);
            var blobContainerName = Environment.GetEnvironmentVariable(BLOB_CONTAINER_NAME);

            var eventHubConnectionString = Environment.GetEnvironmentVariable(EVENT_HUB_CONNECTION_STRING);
            var eventHubName = Environment.GetEnvironmentVariable(EVENT_HUB_NAME);
            var eventHubConsumerGroup = "statsprocessor";

            var blobStorageClient = new BlobContainerClient(storageConnectionString, blobContainerName);

            _processor = new EventProcessorClient(blobStorageClient, eventHubConsumerGroup,
                eventHubConnectionString, eventHubName);

            _processor.ProcessEventAsync += ProcessEventHandler;
            _processor.ProcessErrorAsync += ProcessErrorHandler;

            LogEventHubProcessorClientCreated();
        }

        /// <summary>
        ///     Process Event Hub message to sent to New Relic.
        /// </summary>
        /// <param name="eventArgs"></param>
        /// <returns>
        ///     Task.
        /// </returns>
        private async Task ProcessEventHandler(ProcessEventArgs eventArgs)
        {
            try
            {
                Task processingTask = Task.Run(() => {
                    // Parse event body.
                    var deviceMessage = ParseMessage(eventArgs.Data);

                    SendMessageToNewrelic(deviceMessage);
                });

                await Task.WhenAll(processingTask);
            }
            catch (Exception e)
            {
                LogUnexpectedErrorOccured(e);
            }
        }

        /// <summary>
        ///     Handle error.
        /// </summary>
        /// <param name="eventArgs"></param>
        /// <returns>
        ///     Task.
        /// </returns>
        private async Task ProcessErrorHandler(ProcessErrorEventArgs eventArgs)
        {
            Task processingTask = Task.Run(() => {
                LogUnexpectedErrorOccured(eventArgs.Exception);
            });

            await Task.WhenAll(processingTask);
        }

        /// <summary>
        ///     Log starting Event Hub processor.
        /// </summary>
        private void LogStartingEventHubProcessor()
        {
            CustomLogger.Log(
                _logger,
                LogLevel.Information,
                nameof(EventHandler),
                nameof(StartAsync),
                "Starting Event Hub processor..."
            );
        }

        /// <summary>
        ///     Log stopping Event Hub processor.
        /// </summary>
        private void LogStoppingEventHubProcessor()
        {
            CustomLogger.Log(
                _logger,
                LogLevel.Information,
                nameof(EventHandler),
                nameof(StopAsync),
                "Stopping Event Hub processor..."
            );
        }

        /// <summary>
        ///     Parse event Hub message.
        /// </summary>
        /// <param name="eventData"></param>
        /// <returns>
        ///     Device message object.
        /// </returns>
        private DeviceMessage ParseMessage(EventData eventData)
        {
            LogParsingEventHubMessage();

            var deviceMessage = JsonConvert.DeserializeObject<DeviceMessage>(
                eventData.EventBody.ToString());

            LogEventHubMessageParsed();

            return deviceMessage;
        }

        /// <summary>
        ///     Send device message to New Relic.
        /// </summary>
        /// <param name="deviceMessage"></param>
        private void SendMessageToNewrelic(DeviceMessage deviceMessage)
        {
            var agent = NewRelic.Api.Agent.NewRelic.GetAgent();
            var transaction = agent.CurrentTransaction;

            transaction
                .AddCustomAttribute("deviceName", deviceMessage.DeviceName)
                .AddCustomAttribute("deviceValue", deviceMessage.DeviceValue);
        }

        /// <summary>
        ///     Log the creation start of Event Hub
        ///     processor client.
        /// </summary>
        private void LogCreatingEventHubProcessorClient()
        {
            CustomLogger.Log(
                _logger,
                LogLevel.Information,
                nameof(EventHubHandler),
                nameof(CreateProcessorClient),
                "Creating Event Hub processor..."
            );
        }

        /// <summary>
        ///     Log the successful creation of Event Hub
        ///     processor client.
        /// </summary>
        private void LogEventHubProcessorClientCreated()
        {
            CustomLogger.Log(
                _logger,
                LogLevel.Information,
                nameof(EventHubHandler),
                nameof(CreateProcessorClient),
                "Event Hub processor is created successfully."
            );
        }

        /// <summary>
        ///     Log parsing Event Hub message.
        /// </summary>
        private void LogParsingEventHubMessage()
        {
            CustomLogger.Log(
                _logger,
                LogLevel.Information,
                nameof(EventHubHandler),
                nameof(ParseMessage),
                "Parsing Service Bus message..."
            );
        }

        /// <summary>
        ///     Log Event Hub message parsed.
        /// </summary>
        private void LogEventHubMessageParsed()
        {
            CustomLogger.Log(
                _logger,
                LogLevel.Information,
                nameof(EventHubHandler),
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
                nameof(EventHubHandler),
                nameof(ParseMessage),
                $"Unexpected error! Message: {e.Message}. InnerException:{e.InnerException}."
            );
        }
    }
}
