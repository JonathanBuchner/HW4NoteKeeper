using System.Text;
using HW4NoteKeeperEx2.Interfaces;
using Microsoft.ApplicationInsights;
using Newtonsoft.Json;

namespace HW4NoteKeeperEx2.ApplicationInsightsTrackers
{
    /// <summary>
    /// Application insights exception tracker.
    /// </summary>
    /// <typeparam name="T">The payload type.</typeparam>
    public class ApplicationLogger<T>(TelemetryClient telemetryClient, ILogger logger) : IApplicationLogger<T>
    {
        /// <summary>
        /// Telemetry client instance
        /// </summary>
        private readonly TelemetryClient _telemetryClient = telemetryClient;

        /// <summary>
        /// Logger instance
        /// </summary>
        private readonly ILogger _logger = logger;

        /// <summary>
        /// Track exception through application insights.
        /// </summary>
        /// <param name="ex">Exception to log</param>
        public void LogException(Exception ex)
        {
            LogException(ex, default);
        }

        /// <summary>
        /// Track exception through application insights.
        /// </summary>
        /// <param name="ex">Exception to log</param>
        /// <param name="payload">Payload. The payload may be null to support default(T)</param>
        public void LogException(Exception ex, T? payload)
        {
            ArgumentNullException.ThrowIfNull(ex);

            // Set properties for tracking event telemetry
            var properties = new Dictionary<string, string>
            {
                { "Type", ex.GetType().ToString() },
                { "Message", ex.Message },
                { "StackTrace", ex.StackTrace ?? "No stack trace." }
            };

            // Add payload to properties if not null
            if (payload != null)
            {
                properties.Add("InputPayload", JsonConvert.SerializeObject(payload));
            }

            // Add properties to string for log exception
            var propertiesSb = new StringBuilder();
            propertiesSb.AppendLine(" Error Properties:");
            foreach (var item in properties)
            {
                propertiesSb.AppendLine($" {item.Key}: {item.Value};");
            }
            propertiesSb.AppendLine(" End of error properties.");

            _logger.LogError(ex, message: ex.Message + propertiesSb.ToString());

            _telemetryClient.TrackException(ex, properties);
        }

        /// <summary>
        /// Log warning through application insights and logger.
        /// </summary>
        /// <param name="WarningMessage">Warning message</param>
        public void LogWarning(string WarningMessage)
        {
            LogWarning(WarningMessage, default, null);
        }

        /// <summary>
        /// Log warning through application insights and logger.
        /// </summary>
        /// <param name="WarningMessage">Warning message</param>
        /// <param name="additionalData">Additoinal data to log.</param>
        public void LogWarning(string WarningMessage, Dictionary<string, object> additionalData)
        {
            LogWarning(WarningMessage, default, additionalData);
        }

        /// <summary>
        /// Log warning through application insights and logger.
        /// </summary>
        /// <param name="WarningMessage">Warning message</param>
        /// <param name="payload">Ojbect that caused warning</param>
        public void LogWarning(string WarningMessage, T payload)
        {
            LogWarning(WarningMessage, payload, null);
        }

        /// <summary>
        /// Log warning through application insights and logger.
        /// </summary>
        /// <param name="WarningMessage">Warning message</param>
        /// <param name="payload">Object that caused the warning. May be null</param>
        /// <param name="additionalData">Additoinal data to log.</param>
        public void LogWarning(string WarningMessage, T? payload, Dictionary<string, object>? additionalData = null)
        {
            ArgumentNullException.ThrowIfNull(WarningMessage);

            var properties = new Dictionary<string, string>
            {
                { "WarningMessage", WarningMessage }
            };

            if (payload != null)
            {
                properties.Add("InputPayload", JsonConvert.SerializeObject(payload));
            }
            
            var additionalDataSb = new StringBuilder();
            if (additionalData != null)
            {
                additionalDataSb.AppendLine("  Additional data:");
                
                foreach (var item in additionalData)
                {
                    // Add to addtiona data string for log warning
                    additionalDataSb.AppendLine($" {item.Key}: {item.Value};");

                    // Add properties for tracking event telemetry
                    properties.Add(item.Key, item.Value?.ToString() ?? "");
                }

                additionalDataSb.AppendLine(" End of additional data.");
            }

            _logger.LogWarning(WarningMessage + additionalDataSb.ToString());

            _telemetryClient.TrackTrace("Warning", properties);
        }

        /// <summary>
        /// Log information through application insights. Also logs information to logger.
        /// </summary>
        /// <param name="message">Message to log</param>
        public void LogInformation(string message)
        {
            LogInformation(message, default!, null!);
        }

        /// <summary>
        /// Log information through application insights. Also logs information to logger.
        /// </summary>
        /// <param name="message">Message to log</param>
        /// <param name="additionalData">Additional information to log</param>
        public void LogInformation(string message, Dictionary<string, object> additionalData)
        {
            LogInformation(message, default!, additionalData);
        }

        /// <summary>
        /// Log information through application insights. Also logs information to logger.
        /// </summary>
        /// <param name="message">Message to log</param>
        /// <param name="payload">payload</param>
        public void LogInformation(string message, T payload)
        {
            LogInformation(message, payload, null!);
        }

        /// <summary>
        /// Log information through application insights. Also logs information to logger.
        /// </summary>
        /// <param name="message">Message to log</param>
        /// <param name="payload">payload</param>
        /// <param name="additionalData">Additional information to log</param>
        public void LogInformation(string message, T payload, Dictionary<string, object> additionalData)
        {
            ArgumentNullException.ThrowIfNull(message);
            
            var properties = new Dictionary<string, string>
            {
                { "Message", message }
            };
            
            if (payload != null)
            {
                properties.Add("InputPayload", JsonConvert.SerializeObject(payload));
            }

            var additionalDataSb = new StringBuilder();
            if (additionalData != null)
            {
                additionalDataSb.AppendLine("  Additional data:");

                foreach (var item in additionalData)
                {
                    // Add to addtiona data string for log warning
                    additionalDataSb.AppendLine($" {item.Key}: {item.Value};");
                    // Add properties for tracking event telemetry
                    properties.Add(item.Key, item.Value?.ToString() ?? "");
                }
                additionalDataSb.AppendLine(" End of additional data.");
            }

            _logger.LogInformation(message + additionalDataSb.ToString());
            _telemetryClient.TrackTrace("Information", properties);
        }
    }
}
