using Azure.Identity;
using Azure.Messaging.EventHubs.Producer;
using System;

namespace InputProcessor.EventHub
{
    public class EventHubHandler
    {
        private const string EVENT_HUB_NAMESPACE_NAME = "EVENT_HUB_NAMESPACE_NAME";
        private const string EVENT_HUB_NAME = "EVENT_HUB_NAME";

        public EventHubHandler()
        {

        }

        public EventHubProducerClient CreateProducerClient()
        {
            var eventHubNamespaceName = Environment.GetEnvironmentVariable(EVENT_HUB_NAMESPACE_NAME);
            var eventHubName = Environment.GetEnvironmentVariable(EVENT_HUB_NAME);

            // Create Event Hub connection.
            Console.WriteLine($"{DateTime.UtcNow}: Creating Event Hub connection ...");

            var producerClient = new EventHubProducerClient(
                $"{eventHubNamespaceName}.servicebus.windows.net",
                eventHubName,
                new ManagedIdentityCredential()
            );

            Console.WriteLine($"{DateTime.UtcNow}: -> Event Hub connection is created successfully.{Environment.NewLine}");

            return producerClient;
        }
    }
}