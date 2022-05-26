using System;
using Microsoft.Extensions.Logging;

namespace InputProcessor.Commons
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
            switch(logLevel)
            {
                case LogLevel.Information:
                    logger.LogInformation(
                        message,
                        className,
                        methodName,
                        deviceName,
                        value
                    );
                    break;

                case LogLevel.Debug:
                    logger.LogDebug(
                        message,
                        className,
                        methodName,
                        deviceName,
                        value
                    );
                    break;

                case LogLevel.Warning:
                    logger.LogWarning(
                        message,
                        className,
                        methodName,
                        deviceName,
                        value
                    );
                    break;

                case LogLevel.Error:
                    logger.LogError(
                        message,
                        className,
                        methodName,
                        deviceName,
                        value
                    );
                    break;

                default:
                    break;
            }
        }
    }
}
