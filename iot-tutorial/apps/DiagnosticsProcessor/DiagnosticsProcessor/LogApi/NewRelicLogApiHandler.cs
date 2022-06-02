using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using DiagnosticsProcessor.Commons;
using Microsoft.Extensions.Logging;

namespace DiagnosticsProcessor.LogApi
{
    public class NewRelicLogApiHandler
    {
        private const string NEWRELIC_LOG_URI =
            "https://log-api.eu.newrelic.com/log/v1";

        private readonly ILogger _logger;

        private HttpClient _httpClient;

        public NewRelicLogApiHandler(
            ILogger logger
        )
        {
            // Set logger.
            _logger = logger;

            // Create HTTP client.
            CreateHttpClient();
        }

        /// <summary>
        ///     Create HTTP client.
        /// </summary>
        private void CreateHttpClient()
        {
            _httpClient = new HttpClient();

            // Add New Relic license key.
            var newRelicLicenseKey = Environment
                .GetEnvironmentVariable("NEWRELIC_LICENSE_KEY");

            _httpClient.DefaultRequestHeaders
                .Add("Api-Key", newRelicLicenseKey);
        }

        /// <summary>
        ///     Send log to New Relic.
        /// </summary>
        /// <param name="log">
        ///     Log to be sent to New Relic.
        /// </param>
        public async Task SendLog(
            string log
        )
        {
            var httpContent = new StringContent(log,
                Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(NEWRELIC_LOG_URI,
                httpContent);

            if (response.IsSuccessStatusCode)
            {
                var responseMessage = await response.Content
                    .ReadAsStringAsync();

                LogMessageToNewRelicSent((int)response.StatusCode,
                    responseMessage);
            }
            else
            {
                LogMessageToNewRelicNotSent((int)response.StatusCode,
                    response.ReasonPhrase);
            }
        }

        /// <summary>
        ///     Log message sent to New Relic.
        /// </summary>
        /// <param name="httpStatusCode">
        ///     HTTP status code.
        /// </param>
        /// <param name="responseMessage">
        ///     Response message from New Relic.
        /// </param>
        private void LogMessageToNewRelicSent(
            int httpStatusCode,
            string responseMessage
        )
        {
            CustomLogger.Log(
                _logger,
                LogLevel.Information,
                nameof(NewRelicLogApiHandler),
                nameof(SendLog),
                "Message is sent to New Relic successfully.",
                data: $"statusCode:{httpStatusCode}," +
                $"responseMessage:{responseMessage}"
            );
        }

        /// <summary>
        ///     Log message not sent to New Relic.
        /// </summary>
        /// <param name="httpStatusCode">
        ///     HTTP status code.
        /// </param>
        /// <param name="responseMessage">
        ///     Response message from New Relic.
        /// </param>
        private void LogMessageToNewRelicNotSent(
            int httpStatusCode,
            string responseMessage
        )
        {
            CustomLogger.Log(
                _logger,
                LogLevel.Error,
                nameof(NewRelicLogApiHandler),
                nameof(SendLog),
                "Message is sent to New Relic successfully.",
                data: $"statusCode:{httpStatusCode}," +
                $"responseMessage:{responseMessage}"
            );
        }
    }
}
