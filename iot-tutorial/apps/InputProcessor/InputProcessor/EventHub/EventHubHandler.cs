using Azure.Messaging.EventHubs;
using Azure.Messaging.EventHubs.Producer;
using InputProcessor.Models;
using System;
using System.Threading.Tasks;

namespace InputProcessor.EventHub
{
    public class EventHubHandler
    {
        private const string EVENT_HUB_CONNECTION_STRING = "EVENT_HUB_CONNECTION_STRING";
        private const string EVENT_HUB_NAME = "EVENT_HUB_NAME";

        private EventHubProducerClient _eventHubProducerClient;

        public EventHubHandler()
        {

        }

        public void CreateProducerClient()
        {
            var eventHubNamespaceName = Environment.GetEnvironmentVariable(EVENT_HUB_CONNECTION_STRING);
            var eventHubName = Environment.GetEnvironmentVariable(EVENT_HUB_NAME);

            // Create Event Hub connection.
            Console.WriteLine($"{DateTime.UtcNow}: Creating Event Hub connection ...");

            _eventHubProducerClient = new EventHubProducerClient(eventHubNamespaceName, eventHubName);

            Console.WriteLine($"{DateTime.UtcNow}: -> Event Hub connection is created successfully.{Environment.NewLine}");
        }

        public async Task SendMessage(string messageBody)
        {
            var eventBatch = await _eventHubProducerClient.CreateBatchAsync();

            var eventData = new EventData(messageBody);

            if (eventBatch.TryAdd(eventData))
                Console.WriteLine($"{DateTime.UtcNow}: Event is successfully added to batch.{Environment.NewLine}");
            else
                throw new Exception($"{DateTime.UtcNow}: Event is failed to be added to batch.{Environment.NewLine}");

            await _eventHubProducerClient.SendAsync(eventBatch);
        }
    }
}