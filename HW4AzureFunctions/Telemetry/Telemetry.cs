using HW4AzureFunctionsEx2.MessageProcessors;
using Microsoft.ApplicationInsights;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HW4AzureFunctionsEx2.Telemetry
{
    /// <summary>
    /// Telemetry logging class.
    /// </summary>
    public class Telemetry : ITelemetry
    {
        /// <summary>
        /// Telemetry client instance for logging.
        /// </summary>
        private readonly TelemetryClient _telemetryClient;

        /// <summary>
        /// Logger instance for logging.
        /// </summary>
        private readonly ILogger<TelemetryClient> _logger;

        public Telemetry(TelemetryClient telemetryClient, ILogger<TelemetryClient> logger)
        {
            _telemetryClient = telemetryClient;
            _logger = logger;
        }

        /// <summary>
        /// Logs an information message.
        /// </summary>
        /// <param name="message">message</param>
        public void LogInformation(string message)
        {
            _telemetryClient.TrackTrace(message);
            _logger.LogInformation(message);
        }

        /// <summary>
        /// Logs a success message.
        /// </summary>
        /// <param name="message">message</param>
        public void LogSuccess(string message)
        {
            _telemetryClient.TrackEvent("Success", new Dictionary<string, string> { { "Message", message } });
            _logger.LogInformation(message);
        }

        /// <summary>
        /// Logs a warning message.
        /// </summary>
        /// <param name="message">message</param>
        public void LogWarning(string message)
        {
            _telemetryClient.TrackTrace(message, Microsoft.ApplicationInsights.DataContracts.SeverityLevel.Warning);
            _logger.LogWarning(message);
        }

        /// <summary>
        /// Logs a warning message with additional data.
        /// </summary>
        /// <param name="message">message</param>
        /// <param name="exception">exception</param>
        public void LogError(string message, Exception exception)
        {
            _telemetryClient.TrackException(exception, new Dictionary<string, string> { { "Message", message } });
            _logger.LogError(exception, message);
        }
    }
}
