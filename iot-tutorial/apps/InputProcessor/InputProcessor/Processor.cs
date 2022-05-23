using Azure.Messaging.EventHubs;
using Azure.Messaging.EventHubs.Producer;
using Azure.Messaging.ServiceBus;
using InputProcessor.EventHub;
using InputProcessor.ServiceBus;
using System;
using System.Threading.Tasks;

namespace InputProcessor
{
    public class Processor
    {
        private const int MAX_WAIT_TIME_FOR_MESSAGE = 30;

        private ServiceBusReceiver _serviceBusReceiver;
        private EventHubProducerClient _eventHubProducerClient;

        public Processor()
        {

        }

        public async Task Run()
        {
            try
            {
                InitializeServiceBus();
                InitializeEventHub();

                // Listen to messages on the queue.
                while (true)
                {
                    // Receive the message on the queue.
                    var receivedMessage = await ReceiveMessageFromServiceBus();

                    // Process the message if exists.
                    if (receivedMessage != null)
                    {
                        // Prompt the message.
                        LogServiceBusMessage(receivedMessage);

                        // Send the message to Event Hub.
                        await SendMessageToEventHub(receivedMessage);

                        // Acknowledge message to remove from queue.
                        await AcknowledgeServiceBusMessage(receivedMessage);
                    }
                    else
                        Console.WriteLine($"{DateTime.UtcNow}: No message is found on the queue.{Environment.NewLine}");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"{e}");
            }
        }

        private void InitializeServiceBus()
            => _serviceBusReceiver = new ServiceBusHandler().CreateReceiver();

        private void InitializeEventHub()
            => _eventHubProducerClient = new EventHubHandler().CreateProducerClient();

        private async Task<ServiceBusReceivedMessage> ReceiveMessageFromServiceBus()
        {
            Console.WriteLine($"{DateTime.UtcNow}: Receiving message from the Service Bus ...{Environment.NewLine}");

            return await _serviceBusReceiver.ReceiveMessageAsync(
                maxWaitTime: TimeSpan.FromSeconds(MAX_WAIT_TIME_FOR_MESSAGE)
            );
        }

        private void LogServiceBusMessage(ServiceBusReceivedMessage receivedMessage)
        {
            Console.WriteLine($"{DateTime.UtcNow}: Message is retrieved from the queue.{Environment.NewLine}");
            Console.WriteLine($"{DateTime.UtcNow}: {receivedMessage.Body}{Environment.NewLine}");
        }

        private async Task SendMessageToEventHub(ServiceBusReceivedMessage receivedMessage)
        {
            var eventBatch = await _eventHubProducerClient.CreateBatchAsync();

            var eventData = new EventData(receivedMessage.Body);

            if (eventBatch.TryAdd(eventData))
                Console.WriteLine($"{DateTime.UtcNow}: Event is successfully added to batch.{Environment.NewLine}");
            else
                throw new Exception($"{DateTime.UtcNow}: Event is failed to be added to batch.{Environment.NewLine}");

            await _eventHubProducerClient.SendAsync(eventBatch);
        }

        private async Task AcknowledgeServiceBusMessage(ServiceBusReceivedMessage receivedMessage)
            => await _serviceBusReceiver.CompleteMessageAsync(receivedMessage);
    }
}