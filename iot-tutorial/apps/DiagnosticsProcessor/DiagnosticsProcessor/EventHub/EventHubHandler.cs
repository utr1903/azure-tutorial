using System;
using System.Threading;
using System.Threading.Tasks;
using Azure.Messaging.EventHubs;
using Azure.Messaging.EventHubs.Processor;
using Azure.Storage.Blobs;
using Commons.Logging;
using DiagnosticsProcessor.Exceptions;
using DiagnosticsProcessor.LogApi;
using DiagnosticsProcessor.LogParser;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace DiagnosticsProcessor.EventHub
{
    public class EventHubHandler : IHostedService
    {
        private readonly ILogger _logger;

        private AzureLogParserHandler _azureLogParserHandler;
        private NewRelicLogApiHandler _newRelicLogApiHandler;

        private EventProcessorClient _processor;

        public EventHubHandler(
            ILogger<EventHubHandler> logger
        )
        {
            // Set logger.
            _logger = logger;

            // Create Azure log parser handler.
            CreateAzureLogParserHandler();

            // Create New Relic log API handler.
            CreateNewRelicLogApiHandler();

            // Create Event Hub processor client.
            CreateProcessorClient();
        }

        public async Task StartAsync(
            CancellationToken cancellationToken
        )
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

        public async Task StopAsync(
            CancellationToken cancellationToken
        )
        {
            Task processingTask = Task.Run(() => {
                LogStoppingEventHubProcessor();
            });

            await Task.WhenAll(processingTask);
        }

        /// <summary>
        ///     Create Azure log parser handler.
        /// </summary>
        private void CreateAzureLogParserHandler()
            => _azureLogParserHandler = new AzureLogParserHandler(_logger);

        /// <summary>
        ///     Create New Relic Log API handler.
        /// </summary>
        private void CreateNewRelicLogApiHandler()
            => _newRelicLogApiHandler = new NewRelicLogApiHandler(_logger);

        /// <summary>
        ///     Creates Event Hub processor client.
        /// </summary>
        public void CreateProcessorClient()
        {
            LogCreatingEventHubProcessorClient();

            var storageConnectionString = Environment
                .GetEnvironmentVariable("STORAGE_ACCOUNT_CONNECTION_STRING");

            var blobContainerName = Environment
                .GetEnvironmentVariable("BLOB_CONTAINER_NAME");

            var eventHubConnectionString = Environment
                .GetEnvironmentVariable("EVENT_HUB_CONNECTION_STRING");

            var eventHubName = Environment
                .GetEnvironmentVariable("EVENT_HUB_NAME");

            var eventHubConsumerGroup = Environment
                .GetEnvironmentVariable("EVENT_HUB_CONSUMER_GROUP_NAME");
            
            var blobStorageClient = new BlobContainerClient(
                storageConnectionString, blobContainerName);

            _processor = new EventProcessorClient(blobStorageClient,
                eventHubConsumerGroup, eventHubConnectionString, eventHubName);

            _processor.ProcessEventAsync += ProcessEventHandler;
            _processor.ProcessErrorAsync += ProcessErrorHandler;

            LogEventHubProcessorClientCreated();
        }

        /// <summary>
        ///     Process Event Hub message to sent to New Relic.
        /// </summary>
        /// <param name="eventArgs">
        ///     Event Hub event object.
        /// </param>
        /// <returns>
        ///     Task.
        /// </returns>
        private async Task ProcessEventHandler(
            ProcessEventArgs eventArgs
        )
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
                LogRawAzureLogMessageNotParsed(e.Log);
            }
            catch (Exception e)
            {
                LogUnexpectedErrorOccured(e);
            }
        }

        /// <summary>
        ///     Handle error.
        /// </summary>
        /// <param name="eventArgs">
        ///     Event Hub event object.
        /// </param>
        /// <returns>
        ///     Task.
        /// </returns>
        private async Task ProcessErrorHandler(
            ProcessErrorEventArgs eventArgs
        )
        {
            Task processingTask = Task.Run(() => {
                LogUnexpectedErrorOccured(eventArgs.Exception);
            });

            await Task.WhenAll(processingTask);
        }

        /// <summary>
        ///     Parse raw Azure log message and create a dedicated
        ///     New Relic log payload.
        /// </summary>
        /// <param name="eventData"></param>
        /// <returns>
        ///     New Relic log payload as string.
        /// </returns>
        private string ParseMessage(
            EventData eventData
        )
            => _azureLogParserHandler
                .Run(eventData.EventBody.ToString())
                .ToString();

        /// <summary>
        ///     Send log payload to New Relic.
        /// </summary>
        /// <param name="logPayload">
        ///     Log payload to be sent to New Relic.
        /// </param>
        private async Task SendLogToNewRelic(
            string logPayload
        )
            => await _newRelicLogApiHandler.SendLog(logPayload);

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
        ///     Log raw Azure log message not parsed.
        /// </summary>
        private void LogRawAzureLogMessageNotParsed(
            string exception
        )
        {
            CustomLogger.Log(
                _logger,
                LogLevel.Information,
                nameof(AzureLogParserHandler),
                nameof(ProcessEventHandler),
                "Parsing raw Azure log message...",
                exception: exception
            );
        }

        /// <summary>
        ///     Log unexpected error occurred.
        /// </summary>
        private void LogUnexpectedErrorOccured(
            Exception e
        )
        {
            CustomLogger.Log(
                _logger,
                LogLevel.Error,
                nameof(EventHubHandler),
                nameof(ProcessEventHandler),
                $"Unexpected error occurred.",
                exception: $"errorMessage:{e.Message}," +
                $"innerException:{e.InnerException}."
            );
        }
    }
}
