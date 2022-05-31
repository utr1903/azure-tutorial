using System;
namespace DiagnosticsProcessor.Exceptions
{
    public class EventHubMessageNotParsedException : Exception
    {
        public readonly string Log;

        public EventHubMessageNotParsedException(string log)
        {
            Log = log;
        }
    }
}
