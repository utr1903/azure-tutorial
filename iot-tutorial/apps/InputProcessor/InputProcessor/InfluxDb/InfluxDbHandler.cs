using System;
using System.Threading;
using System.Threading.Tasks;
using Commons.Logging;
using Commons.Models;
using InfluxDB.Client;
using InfluxDB.Client.Api.Domain;
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

        public InfluxDbHandler(
            ILogger logger
        )
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
        public async Task WriteMessage(
            DeviceMessage deviceMessage
        )
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

            record += $" value={deviceMessage.DeviceValue}";

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
                logger: _logger,
                logLevel: LogLevel.Information,
                className: nameof(InfluxDbHandler),
                methodName: nameof(CreateClient),
                message: "Establishing connection to Influx DB..."
            );
        }

        /// <summary>
        ///     Log Influx DB connection established.
        /// </summary>
        private void LogInfluxDbConnectionEstablished()
        {
            CustomLogger.Log(
                logger: _logger,
                logLevel: LogLevel.Information,
                className: nameof(InfluxDbHandler),
                methodName: nameof(CreateClient),
                message: "Connection to Influx DB is established successfully."
            );
        }

        /// <summary>
        ///     Log trying Influx DB connection.
        /// </summary>
        private void LogTryingInfluxDbConnection()
        {
            CustomLogger.Log(
                logger: _logger,
                logLevel: LogLevel.Warning,
                className: nameof(InfluxDbHandler),
                methodName: nameof(CreateClient),
                message: "Trying connection to Influx DB..."
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
                logger: _logger,
                logLevel: LogLevel.Information,
                className: nameof(InfluxDbHandler),
                methodName: nameof(WriteMessage),
                message: "Writing to Influx DB...",
                data: $"deviceName:{deviceMessage.DeviceName}," +
                $"deviceValue:{deviceMessage.DeviceValue}"
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
                logger: _logger,
                logLevel: LogLevel.Information,
                className: nameof(InfluxDbHandler),
                methodName: nameof(WriteMessage),
                message: "Message is successfully written into Influx DB.",
                data: $"deviceName:{deviceMessage.DeviceName}," +
                $"deviceValue:{deviceMessage.DeviceValue}"
            );
        }
    }
}
