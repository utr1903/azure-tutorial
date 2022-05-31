using System;
using Microsoft.Extensions.Logging;

namespace DiagnosticsProcessor.Commons
{
    public static class CustomLogger
    {
        public static void Log(
            ILogger logger,
            LogLevel logLevel,
            string className,
            string methodName,
            string message,
            string deviceName = null,
            string value = null
        )
        {
            var log = CreateLogMessage(logLevel, className, methodName, message, deviceName, value);

            switch (logLevel)
            {
                case LogLevel.Information:
                    logger.LogInformation(log);
                    break;

                case LogLevel.Debug:
                    logger.LogDebug(log);
                    break;

                case LogLevel.Warning:
                    logger.LogWarning(log);
                    break;

                case LogLevel.Error:
                    logger.LogError(log);
                    break;

                default:
                    break;
            }
        }

        private static string CreateLogMessage(
            LogLevel logLevel,
            string className,
            string methodName,
            string message,
            string deviceName,
            string value
        )
        {
            var log = $"logLevel:{logLevel}," +
                $"className:{className}," +
                $"methodName:{methodName}," +
                $"message:{message}";

            if (deviceName != null)
                log += $",deviceName:{deviceName}";

            if (value != null)
                log += $",value:{value}";

            return log;
        }
    }
}
