using Azure.Messaging.EventHubs;
using Azure.Messaging.EventHubs.Producer;
using InputProcessor.Commons;
using InputProcessor.Models;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;

namespace InputProcessor.EventHub
{
    public class EventHubHandler
    {
        private readonly ILogger _logger;

        private EventHubProducerClient _eventHubProducerClient;

        public EventHubHandler(ILogger logger)
        {
            _logger = logger;
        }

        /// <summary>
        ///     Create Event Hub producer client.
        /// </summary>
        public void CreateProducerClient()
        {
            LogCreatingEventHubProducerClient();

            var eventHubConnectionString = Environment.GetEnvironmentVariable("EVENT_HUB_CONNECTION_STRING");
            var eventHubName = Environment.GetEnvironmentVariable("EVENT_HUB_NAME");

            // Create Event Hub connection.
            _eventHubProducerClient = new EventHubProducerClient(eventHubConnectionString, eventHubName);

            LogEventHubProducerClientCreated();
        }

        /// <summary>
        ///     Send message to Event Hub.
        /// </summary>
        /// <param name="deviceMessage">
        ///     Device message object.
        /// </param>
        /// <returns>
        ///     Task.
        /// </returns>
        public async Task SendMessage(DeviceMessage deviceMessage)
        {
            var messageAsString = JsonConvert.SerializeObject(deviceMessage);
            var eventBatch = await _eventHubProducerClient.CreateBatchAsync();

            var eventData = new EventData(messageAsString);

            if (eventBatch.TryAdd(eventData))
                LogAddingMessageToBatchSuceeded(deviceMessage);
            else
                LogAddingMessageToBatchFailed(deviceMessage);

            await _eventHubProducerClient.SendAsync(eventBatch);
        }

        /// <summary>
        ///     Log the creation start of Event Hub
        ///     producer client.
        /// </summary>
        private void LogCreatingEventHubProducerClient()
        {
            CustomLogger.Log(
                _logger,
                LogLevel.Information,
                nameof(EventHubHandler),
                nameof(CreateProducerClient),
                "Creating Event Hub connection..."
            );
        }

        /// <summary>
        ///     Log the successful creation of Event Hub
        ///     producer client.
        /// </summary>
        private void LogEventHubProducerClientCreated()
        {
            CustomLogger.Log(
                _logger,
                LogLevel.Information,
                nameof(EventHubHandler),
                nameof(CreateProducerClient),
                "Event Hub connection is created successfully."
            );
        }

        /// <summary>
        ///     Log successful batch addition.
        /// <param name="deviceMessage">
        ///     Device message object.
        /// </param>
        /// </summary>
        private void LogAddingMessageToBatchSuceeded(
            DeviceMessage deviceMessage
        )
        {
            CustomLogger.Log(
                _logger,
                LogLevel.Information,
                nameof(EventHubHandler),
                nameof(SendMessage),
                "Message is successfully added to batch.",
                deviceMessage.DeviceName,
                deviceMessage.DeviceValue.ToString()
            );
        }

        /// <summary>
        ///     Log failed batch addition.
        /// <param name="deviceMessage">
        ///     Device message object.
        /// </param>
        /// </summary>
        private void LogAddingMessageToBatchFailed(
            DeviceMessage deviceMessage
        )
        {
            CustomLogger.Log(
                _logger,
                LogLevel.Error,
                nameof(EventHubHandler),
                nameof(SendMessage),
                "Message is failed to be added to batch.",
                deviceMessage.DeviceName,
                deviceMessage.DeviceValue.ToString()
            );
        }
    }
}