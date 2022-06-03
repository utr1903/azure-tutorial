using Microsoft.Extensions.Logging;

namespace Commons.Logging
{
    public static class CustomLogger
    {
        public static void Log(
            ILogger logger,
            LogLevel logLevel,
            string className,
            string methodName,
            string message,
            string data = null,
            string exception = null
        )
        {
            var log = CreateLogMessage(logLevel, className, methodName, message, data, exception);

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
            string data,
            string exception
        )
        {
            var log = $"logLevel:{logLevel}," +
                $"className:{className}," +
                $"methodName:{methodName}," +
                $"message:{message}";

            if (data != null)
                log += $",data:{data}";

            if (exception != null)
                log += $",exception:{exception}";

            return log;
        }
    }
}
