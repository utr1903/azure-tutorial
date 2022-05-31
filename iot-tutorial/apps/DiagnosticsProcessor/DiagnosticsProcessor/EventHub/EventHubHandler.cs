using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Azure.Messaging.EventHubs;
using Azure.Messaging.EventHubs.Processor;
using Azure.Storage.Blobs;
using DiagnosticsProcessor.Commons;
using DiagnosticsProcessor.Exceptions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DiagnosticsProcessor.EventHub
{
    public class EventHubHandler : IHostedService
    {
        private readonly ILogger _logger;

        private const string NEWRELIC_LOG_URI = "https://log-api.eu.newrelic.com/log/v1";
        private string NEWRELIC_LICENSE_KEY;

        private HttpClient _httpClient;
        private EventProcessorClient _processor;

        public EventHubHandler(ILogger<EventHubHandler> logger)
        {
            // Set logger.
            _logger = logger;

            // Create HTTP client.
            CreateHttpClient();

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
        ///     Create HTTP client.
        /// </summary>
        private void CreateHttpClient()
        {
            _httpClient = new HttpClient();

            // Add an Accept header for JSON format.
            _httpClient.DefaultRequestHeaders.Accept
                .Add(new MediaTypeWithQualityHeaderValue("application/json"));

            // Add New Relic license key.
            _httpClient.DefaultRequestHeaders
                .Add("X-License-Key", NEWRELIC_LICENSE_KEY);
        }

        /// <summary>
        ///     Creates Event Hub processor client.
        /// </summary>
        public void CreateProcessorClient()
        {
            LogCreatingEventHubProcessorClient();

            NEWRELIC_LICENSE_KEY = Environment.GetEnvironmentVariable("NEWRELIC_LICENSE_KEY");
            var storageConnectionString = Environment.GetEnvironmentVariable("STORAGE_ACCOUNT_CONNECTION_STRING");
            var blobContainerName = Environment.GetEnvironmentVariable("BLOB_CONTAINER_NAME");

            var eventHubConnectionString = Environment.GetEnvironmentVariable("EVENT_HUB_CONNECTION_STRING");
            var eventHubName = Environment.GetEnvironmentVariable("EVENT_HUB_NAME");
            var eventHubConsumerGroup = Environment.GetEnvironmentVariable("EVENT_HUB_CONSUMER_GROUP_NAME");
            
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
                // Try to parse log message.
                var log = ParseMessage(eventArgs.Data);

                // Send log to New Relic.
                await SendLogToNewRelic(log);
            }
            catch (EventHubMessageNotParsedException e)
            {
                LogEventHubMessageNotParsed(e.Log);
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
        ///     Log as string.
        /// </returns>
        private string ParseMessage(EventData eventData)
        {
            LogParsingEventHubMessage();

            var log = eventData.EventBody.ToString();
            try
            {
                JsonConvert.DeserializeObject<JObject>(log);

                LogEventHubMessageParsed();

                return log;
            }
            catch
            {
                throw new EventHubMessageNotParsedException(log);
            }
        }

        /// <summary>
        ///     Send log to New Relic.
        /// </summary>
        /// <param name="log">
        ///     Log to be sent to New Relic.
        /// </param>
        private async Task SendLogToNewRelic(string log)
        {
            var httpContent = new StringContent(log);
            var response = await _httpClient.PostAsync(NEWRELIC_LOG_URI, httpContent);

            if (response.IsSuccessStatusCode)
            {
                var responseMessage = await response.Content.ReadAsStringAsync();
                LogMessageToNewRelicSent(responseMessage);
            }
            else
            {
                LogMessageToNewRelicNotSent((int)response.StatusCode,
                    response.ReasonPhrase);
            }
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
                "Parsing Event Hub message..."
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
                "Event Hub message is parsed."
            );
        }

        /// <summary>
        ///     Log Event Hub message not parsed.
        /// </summary>
        /// <param name="log">
        ///     Not parsed log.
        /// </param>
        private void LogEventHubMessageNotParsed(string log)
        {
            CustomLogger.Log(
                _logger,
                LogLevel.Error,
                nameof(EventHubHandler),
                nameof(ParseMessage),
                $"Event Hub log is not parsed: {log}"
            );
        }

        /// <summary>
        ///     Log sending message to New Relic.
        /// </summary>
        private void LogSendingMessageToNewRelic()
        {
            CustomLogger.Log(
                _logger,
                LogLevel.Information,
                nameof(EventHubHandler),
                nameof(SendLogToNewRelic),
                "Sending message to New Relic..."
            );
        }

        /// <summary>
        ///     Log message sent to New Relic.
        /// </summary>
        /// <param name="responseMessage">
        ///     Response message from New Relic.
        /// </param>
        private void LogMessageToNewRelicSent(string responseMessage)
        {
            CustomLogger.Log(
                _logger,
                LogLevel.Information,
                nameof(EventHubHandler),
                nameof(SendLogToNewRelic),
                $"Message is sent to New Relic successfully: {responseMessage}"
            );
        }

        /// <summary>
        ///     Log message not sent to New Relic.
        /// </summary>
        /// <param name="httpStatusCode">
        ///     HTTP status code.
        /// </param>
        /// <param name="responseMessage">
        ///     Response message from New Relic.
        /// </param>
        private void LogMessageToNewRelicNotSent(int httpStatusCode, string responseMessage)
        {
            CustomLogger.Log(
                _logger,
                LogLevel.Error,
                nameof(EventHubHandler),
                nameof(SendLogToNewRelic),
                $"Message is sent to New Relic successfully: {responseMessage}"
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
                nameof(ProcessEventHandler),
                $"Unexpected error! Message: {e.Message}. InnerException:{e.InnerException}."
            );
        }
    }
}
