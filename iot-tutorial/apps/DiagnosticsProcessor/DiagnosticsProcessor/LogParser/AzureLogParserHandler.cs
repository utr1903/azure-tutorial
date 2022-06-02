using System;
using DiagnosticsProcessor.Commons;
using DiagnosticsProcessor.Exceptions;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DiagnosticsProcessor.LogParser
{
    public class AzureLogParserHandler
    {
        private const string NEWRELIC_LOGS_SOURCE = "azure";
        private const string NEWRELIC_LOGS_VERSION = "1.2.1";

        private readonly ILogger _logger;

        private JObject _common;

        public AzureLogParserHandler(
            ILogger logger
        )
        {
            // Set logger.
            _logger = logger;

            // Initialize common block of payload.
            InitializeCommonData();
        }

        private void InitializeCommonData()
        {

            // Create plugin.
            var plugin = new JObject
            {
                { "type", NEWRELIC_LOGS_SOURCE },
                { "version", NEWRELIC_LOGS_VERSION }
            };

            // Create attributes.
            var attributes = new JObject
            {
                { "plugin", plugin }
            };

            // Create common.
            _common = new JObject
            {
                { "attributes", attributes }
            };
        }

        /// <summary>
        ///     Parses the log, adds metadata and creates a payload.
        /// </summary>
        /// <param name="azureLogRaw">
        ///     Raw string Azure log.
        /// </param>
        /// <returns>
        ///     Payload for New Relic.
        /// </returns>
        public JObject Run(
            string azureLogRaw
        )
        {
            // Try to parse the raw log as JSON.
            var azureLog = ParseRawAzureLog(azureLogRaw);

            // Append log with resource specific metadata.
            AddMetadata(azureLog);

            // Create payload to send to New Relic.
            var payload = CreatePayload(azureLog);

            return payload;
        }

        /// <summary>
        ///     Parses the raw Azure log.
        ///     Throws EventHubMessageNotParsedException if not.
        /// </summary>
        /// <param name="azureLogRaw"></param>
        /// <returns>
        ///     Parsed Azure JSON log.
        /// </returns>
        private JObject ParseRawAzureLog(
            string azureLogRaw
        )
        {
            try
            {
                LogParsingRawAzureLogMessage();

                var azureLog = JsonConvert
                    .DeserializeObject<JObject>(azureLogRaw);

                LogRawAzureLogMessageParsed(azureLog.ToString());

                return azureLog;
            }
            catch
            {
                throw new EventHubMessageNotParsedException(azureLogRaw);
            }
        }

        /// <summary>
        ///     Adds Azure specific metadata to log.
        /// </summary>
        /// <param name="azureLog">
        ///     Parsed Azure log.
        /// </param>
        private void AddMetadata(
            JObject azureLog
        )
        {
            LogAddingResourceMetadataToLog();

            var isResourceIdParsed = azureLog.TryGetValue(
                "resourceId",
                StringComparison.OrdinalIgnoreCase,
                out JToken resourceId
            );

            if (isResourceIdParsed)
            {
                var resourceIdAsString = resourceId.ToString().ToLower();

                LogResourceIdPropertyFound(resourceIdAsString);

                if (resourceIdAsString.StartsWith("/subscriptions/"))
                {
                    var metadata = new JObject();

                    var resourceIdAsArray = resourceIdAsString.Split("/");

                    if (resourceIdAsArray.Length > 2)
                        metadata.Add("subscriptionId", resourceIdAsArray[2]);

                    if (resourceIdAsArray.Length > 4)
                        metadata.Add("resourceGroup", resourceIdAsArray[4]);

                    if (resourceIdAsArray.Length > 6)
                        metadata.Add("source", resourceIdAsArray[6]
                            .Replace("microsoft.", "azure."));

                    azureLog.Add("metadata", metadata);

                    LogResourceMetadataAddedToLog();
                }
                else
                {
                    LogResourceIdNotCompatible();
                }
            }
            else
            {
                LogResourceIdPropertyNotFound();
            }
        }

        /// <summary>
        ///     Creates a New Relic payload.
        /// </summary>
        /// <param name="azureLog">
        ///     Parsed Azure log.
        /// </param>
        /// <returns>
        ///     Payload.
        /// </returns>
        private JObject CreatePayload(
            JObject azureLog
        )
        {
            var logs = new JArray
            {
                azureLog
            };

            var payload = new JObject
            {
                { "common", _common },
                { "logs", logs },
            };

            return payload;
        }

        /// <summary>
        ///     Log parsing raw Azure log message.
        /// </summary>
        private void LogParsingRawAzureLogMessage()
        {
            CustomLogger.Log(
                _logger,
                LogLevel.Information,
                nameof(AzureLogParserHandler),
                nameof(ParseRawAzureLog),
                "Parsing raw Azure log message..."
            );
        }

        /// <summary>
        ///     Log Azure log message parsed.
        /// </summary>
        /// <param name="log">
        ///     Parsed & stringified log message.
        /// </param>
        private void LogRawAzureLogMessageParsed(
            string log
        )
        {
            CustomLogger.Log(
                _logger,
                LogLevel.Information,
                nameof(AzureLogParserHandler),
                nameof(ParseRawAzureLog),
                "Raw Azure log message is parsed.",
                data: $"{log}"
            );
        }

        /// <summary>
        ///     Log adding resource metadata to log.
        /// </summary>
        private void LogAddingResourceMetadataToLog()
        {
            CustomLogger.Log(
                _logger,
                LogLevel.Information,
                nameof(AzureLogParserHandler),
                nameof(AddMetadata),
                "Adding resource specific metadata to log..."
            );
        }

        /// <summary>
        ///     Log resource ID property found.
        /// </summary>
        /// <param name="resourceIdAsString">
        ///     Found resourceId value out of the parsed raw log.
        /// </param>
        private void LogResourceIdPropertyFound(
            string resourceIdAsString
        )
        {
            CustomLogger.Log(
                _logger,
                LogLevel.Information,
                nameof(AzureLogParserHandler),
                nameof(AddMetadata),
                "Resource ID property is found.",
                data: $"{resourceIdAsString}"
            );
        }

        /// <summary>
        ///     Log resource ID property not found.
        /// </summary>
        private void LogResourceIdPropertyNotFound()
        {
            CustomLogger.Log(
                _logger,
                LogLevel.Information,
                nameof(AzureLogParserHandler),
                nameof(AddMetadata),
                "Resource ID property is found."
            );
        }

        /// <summary>
        ///     Log resource ID property not compatible to parse
        ///     in a structure.
        /// </summary>
        private void LogResourceIdNotCompatible()
        {
            CustomLogger.Log(
                _logger,
                LogLevel.Information,
                nameof(AzureLogParserHandler),
                nameof(AddMetadata),
                "Resource ID property not compatible to parse in a structure."
            );
        }

        /// <summary>
        ///     Log resource metadata added to log.
        /// </summary>
        private void LogResourceMetadataAddedToLog()
        {
            CustomLogger.Log(
                _logger,
                LogLevel.Information,
                nameof(AzureLogParserHandler),
                nameof(AddMetadata),
                "Resource metadata is added to log."
            );
        }
    }
}
