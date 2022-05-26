using System;
using System.Threading;
using System.Threading.Tasks;
using InfluxDB.Client;
using InfluxDB.Client.Api.Domain;
using InputProcessor.Commons;
using InputProcessor.Models;
using Microsoft.Extensions.Logging;

namespace InputProcessor.InfluxDb
{
    public class InfluxDbHandler
    {
        private readonly ILogger _logger;

        private readonly string INFLUXDB_SERVICE_NAME;
        private readonly string INFLUXDB_NAMESPACE;
        private readonly string INFLUXDB_PORT;

        private readonly string INFLUXDB_ORGANIZATION_NAME;
        private readonly string INFLUXDB_BUCKET_NAME;

        private WriteApiAsync _writeApi;

        public InfluxDbHandler(ILogger logger)
        {
            _logger = logger;
            
            INFLUXDB_SERVICE_NAME = Environment.GetEnvironmentVariable("INFLUXDB_SERVICE_NAME");
            INFLUXDB_NAMESPACE = Environment.GetEnvironmentVariable("INFLUXDB_NAMESPACE");
            INFLUXDB_PORT = Environment.GetEnvironmentVariable("INFLUXDB_PORT");

            INFLUXDB_ORGANIZATION_NAME = Environment.GetEnvironmentVariable("INFLUXDB_ORGANIZATION_NAME");
            INFLUXDB_BUCKET_NAME = Environment.GetEnvironmentVariable("INFLUXDB_BUCKET_NAME");
        }

        /// <summary>
        ///     Create Influx DB client.
        /// </summary>
        public void CreateClient()
        {
            LogEstablishingInfluxDbConnection();

            var influxDbClient = InfluxDBClientFactory.Create(
                $"http://{INFLUXDB_SERVICE_NAME}.{INFLUXDB_NAMESPACE}.svc.cluster.local:{INFLUXDB_PORT}",
                "admin",
                "admin123".ToCharArray()
            );

            while (true)
            {
                var isConnectionEstablished = influxDbClient.PingAsync().Result;
                if (isConnectionEstablished)
                {
                    LogInfluxDbConnectionEstablished();
                    break;
                }
                else
                {
                    LogTryingInfluxDbConnection();
                    Thread.Sleep(3000);
                }
            }
            
            _writeApi = influxDbClient.GetWriteApiAsync();
        }

        /// <summary>
        ///     Write message into Influx DB.
        /// </summary>
        /// <param name="deviceMessage">
        ///     Device message object.
        /// </param>
        /// <returns>
        ///     Task.
        /// </returns>
        public async Task WriteMessage(DeviceMessage deviceMessage)
        {
            LogWritingMessageToInfluxDb(deviceMessage);

            var record = $"{deviceMessage.DeviceName},";

            var counter = 0;
            foreach (var tag in deviceMessage.Tags)
            {
                ++counter;
                record += $"{tag.Key}={tag.Value}";

                if (counter != deviceMessage.Tags.Keys.Count)
                    record += ",";
            }

            record += $" value={deviceMessage.Value}";

            await _writeApi.WriteRecordAsync(record, WritePrecision.Ns,
                org: INFLUXDB_ORGANIZATION_NAME,
                bucket: INFLUXDB_BUCKET_NAME);

            LogMessageWrittenToInfluxDb(deviceMessage);
        }

        /// <summary>
        ///     Log establishing Influx DB connection.
        /// </summary>
        private void LogEstablishingInfluxDbConnection()
        {
            CustomLogger.Log(
                _logger,
                LogLevel.Information,
                nameof(InfluxDbHandler),
                nameof(CreateClient),
                "Establishing connection to Influx DB..."
            );
        }

        /// <summary>
        ///     Log Influx DB connection established.
        /// </summary>
        private void LogInfluxDbConnectionEstablished()
        {
            CustomLogger.Log(
                _logger,
                LogLevel.Information,
                nameof(InfluxDbHandler),
                nameof(CreateClient),
                "Connection to InfluxDB is established successfully."
            );
        }

        /// <summary>
        ///     Log trying Influx DB connection.
        /// </summary>
        private void LogTryingInfluxDbConnection()
        {
            CustomLogger.Log(
                _logger,
                LogLevel.Warning,
                nameof(InfluxDbHandler),
                nameof(CreateClient),
                "Trying connection to Influx DB..."
            );
        }

        /// <summary>
        ///     Log writing to Influx DB.
        /// </summary>
        /// <param name="deviceMessage">
        ///     Device message object.
        /// </param>
        private void LogWritingMessageToInfluxDb(
            DeviceMessage deviceMessage
        )
        {
            CustomLogger.Log(
                _logger,
                LogLevel.Information,
                nameof(InfluxDbHandler),
                nameof(WriteMessage),
                "Writing to Influx DB...",
                deviceMessage.DeviceName,
                deviceMessage.Value.ToString()
            );
        }

        /// <summary>
        ///     Log message written successfully into
        ///     Influx DB.
        /// </summary>
        /// <param name="deviceMessage"></param>
        private void LogMessageWrittenToInfluxDb(
            DeviceMessage deviceMessage
        )
        {
            CustomLogger.Log(
                _logger,
                LogLevel.Information,
                nameof(InfluxDbHandler),
                nameof(WriteMessage),
                "Message is successfully written into Influx DB.",
                deviceMessage.DeviceName,
                deviceMessage.Value.ToString()
            );
        }
    }
}
