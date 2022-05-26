using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Azure.Messaging.EventHubs;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace StatsProcessor
{
    public class StatsProcessor
    {
        public StatsProcessor()
        {
            
        }

        [FunctionName("StatsProcessor")]
        public async Task Run(
            [
                EventHubTrigger("%EVENT_HUB_NAME%",
                    Connection = "EventHubConnection"
                )
                //Connection = "Endpoint=sb://<service-bus-resource>.servicebus.windows.net;Authentication=Managed Identity;")
            ] EventData[] events,
            ILogger log
        )
        {
            var exceptions = new List<Exception>();

            //IAgent agent = NewRelic.Api.Agent.NewRelic.GetAgent();
            //ITransaction transaction = agent.CurrentTransaction;

            //transaction
            //    .AddCustomAttribute("Discount Code", "Summer Super Sale")
            //    .AddCustomAttribute("Item Code", 31456);

            foreach (EventData eventData in events)
            {
                try
                {
                    string messageBody = eventData.EventBody.ToString();


                    // Replace these two lines with your processing logic.
                    log.LogInformation($"C# Event Hub trigger function processed a message: {messageBody}");
                    await Task.Yield();
                }
                catch (Exception e)
                {
                    // We need to keep processing the rest of the batch - capture this exception and continue.
                    // Also, consider capturing details of the message that failed processing so it can be processed again later.
                    exceptions.Add(e);
                }
            }

            // Once processing of the batch is complete, if any messages in the batch failed processing throw an exception so that there is a record of the failure.

            if (exceptions.Count > 1)
                throw new AggregateException(exceptions);

            if (exceptions.Count == 1)
                throw exceptions.Single();
        }
    }
}
