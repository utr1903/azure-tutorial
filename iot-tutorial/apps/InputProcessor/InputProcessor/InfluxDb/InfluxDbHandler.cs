using System;
using System.Threading;
using System.Threading.Tasks;
using InfluxDB.Client;
using InfluxDB.Client.Api.Domain;
using InfluxDB.Client.Writes;
using InputProcessor.Models;

namespace InputProcessor.InfluxDb
{
    public class InfluxDbHandler
    {
        private const string INFLUXDB_SERVICE_NAME = "INFLUXDB_SERVICE_NAME";
        private const string INFLUXDB_NAMESPACE = "INFLUXDB_NAMESPACE";
        private const string INFLUXDB_PORT = "INFLUXDB_PORT";

        private const string INFLUXDB_ORGANIZATION_NAME = "INFLUXDB_ORGANIZATION_NAME";
        private const string INFLUXDB_BUCKET_NAME = "INFLUXDB_BUCKET_NAME";

        private WriteApiAsync _writeApi;

        public InfluxDbHandler()
        {

        }

        public void CreateClient()
        {
            Console.WriteLine("Connecting to InfluxDB...");

            var influxDbServiceName = Environment.GetEnvironmentVariable(INFLUXDB_SERVICE_NAME);
            var influxDbNamespace = Environment.GetEnvironmentVariable(INFLUXDB_NAMESPACE);
            var influxDbPort = Environment.GetEnvironmentVariable(INFLUXDB_PORT);

            var influxDbClient = InfluxDBClientFactory.Create(
                $"http://{influxDbServiceName}.{influxDbNamespace}.svc.cluster.local:{influxDbPort}",
                "admin",
                "admin123".ToCharArray()
            );

            while (true)
            {
                Console.WriteLine("Establishing connection to InfluxDB...");

                var isConnectionEstablished = influxDbClient.PingAsync().Result;
                if (isConnectionEstablished)
                {
                    Console.WriteLine(" -> Connection to InfluxDB is established successfully.");
                    break;
                }
                else
                {
                    Console.WriteLine(" -> Trying...");
                    Thread.Sleep(3000);
                }
            }
            
            _writeApi = influxDbClient.GetWriteApiAsync();
        }

        public async Task WriteMessage(DeviceMessage deviceMessage)
        {
            var point = PointData.Measurement(deviceMessage.DeviceName)
                .Tag("location", "west")
                .Field("value", deviceMessage.Value)
                .Timestamp(DateTime.UtcNow.AddSeconds(-10), WritePrecision.Ns);

            foreach (var tag in deviceMessage.Tags)
                point.Tag(tag.Key, tag.Value);

            await _writeApi.WritePointAsync(point, INFLUXDB_BUCKET_NAME,
                INFLUXDB_ORGANIZATION_NAME);
        }
    }
}
