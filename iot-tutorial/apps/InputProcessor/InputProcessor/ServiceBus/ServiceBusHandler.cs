using Azure.Messaging.ServiceBus;
using System;

namespace InputProcessor.ServiceBus
{
    public class ServiceBusHandler
    {
        private const string SERVICE_BUS_CONNECTION_STRING = "SERVICE_BUS_CONNECTION_STRING";
        private const string SERVICE_BUS_QUEUE_NAME = "SERVICE_BUS_QUEUE_NAME";

        public ServiceBusHandler()
        {

        }

        public ServiceBusReceiver CreateReceiver()
        {
            var serviceBusConnectionString = Environment.GetEnvironmentVariable(SERVICE_BUS_CONNECTION_STRING);
            var serviceBusQueueName = Environment.GetEnvironmentVariable(SERVICE_BUS_QUEUE_NAME);

            // Create Service Bus connection.
            Console.WriteLine($"{DateTime.UtcNow}: Creating Service Bus connection ...");

            var client = new ServiceBusClient(serviceBusConnectionString);

            Console.WriteLine($"{DateTime.UtcNow}: -> Service Bus connection is created successfully.{Environment.NewLine}");

            // create a receiver that we can use to receive the message
            var serviceBusReceiverOptions = new ServiceBusReceiverOptions
            {
                ReceiveMode = ServiceBusReceiveMode.PeekLock
            };

            // Create the receiver.
            Console.WriteLine($"{DateTime.UtcNow}: Creating Receiver client ...");

            var receiver = client.CreateReceiver(serviceBusQueueName, serviceBusReceiverOptions);

            Console.WriteLine($"{DateTime.UtcNow}: -> Receiver client is created successfully.{Environment.NewLine}");

            return receiver;
        }
    }
}