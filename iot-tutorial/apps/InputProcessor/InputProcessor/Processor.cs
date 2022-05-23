using Azure.Messaging.ServiceBus;
using InputProcessor.ServiceBus;
using System;
using System.Text;
using System.Threading.Tasks;

namespace InputProcessor
{
    public class Processor
    {
        private const int MAX_WAIT_TIME_FOR_MESSAGE = 30;

        private ServiceBusReceiver _serviceBusReceiver;

        public Processor()
        {

        }

        public async Task Run()
        {
            try
            {
                InitializeServiceBus();

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

        private async Task AcknowledgeServiceBusMessage(ServiceBusReceivedMessage receivedMessage)
            => await _serviceBusReceiver.CompleteMessageAsync(receivedMessage);
    }
}