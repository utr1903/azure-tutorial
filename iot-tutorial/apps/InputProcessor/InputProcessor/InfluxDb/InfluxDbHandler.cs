using System;
using System.Threading;
using System.Threading.Tasks;
using InfluxDB.Client;
using InfluxDB.Client.Api.Domain;
using InputProcessor.Models;

namespace InputProcessor.InfluxDb
{
    public class InfluxDbHandler
    {
        private readonly string INFLUXDB_SERVICE_NAME;
        private readonly string INFLUXDB_NAMESPACE;
        private readonly string INFLUXDB_PORT;

        private readonly string INFLUXDB_ORGANIZATION_NAME;
        private readonly string INFLUXDB_BUCKET_NAME;

        private WriteApiAsync _writeApi;

        public InfluxDbHandler()
        {
            INFLUXDB_SERVICE_NAME = Environment.GetEnvironmentVariable("INFLUXDB_SERVICE_NAME");
            INFLUXDB_NAMESPACE = Environment.GetEnvironmentVariable("INFLUXDB_NAMESPACE");
            INFLUXDB_PORT = Environment.GetEnvironmentVariable("INFLUXDB_PORT");

            INFLUXDB_ORGANIZATION_NAME = Environment.GetEnvironmentVariable("INFLUXDB_ORGANIZATION_NAME");
            INFLUXDB_BUCKET_NAME = Environment.GetEnvironmentVariable("INFLUXDB_BUCKET_NAME");
        }

        public void CreateClient()
        {
            Console.WriteLine($"{DateTime.UtcNow}: Connecting to InfluxDB...{Environment.NewLine}");

            var influxDbClient = InfluxDBClientFactory.Create(
                $"http://{INFLUXDB_SERVICE_NAME}.{INFLUXDB_NAMESPACE}.svc.cluster.local:{INFLUXDB_PORT}",
                "admin",
                "admin123".ToCharArray()
            );

            while (true)
            {
                Console.WriteLine("Establishing connection to InfluxDB...");

                var isConnectionEstablished = influxDbClient.PingAsync().Result;
                if (isConnectionEstablished)
                {
                    Console.WriteLine($"{DateTime.UtcNow}: " +
                        $"Connection to InfluxDB is established successfully.{Environment.NewLine}");
                    break;
                }
                else
                {
                    Console.WriteLine($"{DateTime.UtcNow}: Trying...{Environment.NewLine}");
                    Thread.Sleep(3000);
                }
            }
            
            _writeApi = influxDbClient.GetWriteApiAsync();
        }

        public async Task WriteMessage(DeviceMessage deviceMessage)
        {
            Console.WriteLine($"{DateTime.UtcNow}: Writing message to InfluxDB...{Environment.NewLine}");

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

            Console.WriteLine($"{DateTime.UtcNow}: Message is successfully written.{Environment.NewLine}");
        }
    }
}
