using Azure.Messaging.EventHubs.Producer;
using System;

namespace InputProcessor.EventHub
{
    public class EventHubHandler
    {
        private const string EVENT_HUB_CONNECTION_STRING = "EVENT_HUB_CONNECTION_STRING";
        private const string EVENT_HUB_NAME = "EVENT_HUB_NAME";

        public EventHubHandler()
        {

        }

        public EventHubProducerClient CreateProducerClient()
        {
            var eventHubNamespaceName = Environment.GetEnvironmentVariable(EVENT_HUB_CONNECTION_STRING);
            var eventHubName = Environment.GetEnvironmentVariable(EVENT_HUB_NAME);

            // Create Event Hub connection.
            Console.WriteLine($"{DateTime.UtcNow}: Creating Event Hub connection ...");

            var producerClient = new EventHubProducerClient(eventHubNamespaceName, eventHubName);

            Console.WriteLine($"{DateTime.UtcNow}: -> Event Hub connection is created successfully.{Environment.NewLine}");

            return producerClient;
        }
    }
}