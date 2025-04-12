using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HW4NoteKeeperWebJobTelemetry
{
    /// <summary>
    /// Interface for telemetry logging.
    /// </summary>
    public interface ITelemetry
    {
        /// <summary>
        /// Logs an information message.
        /// </summary>
        /// <param name="message">message</param>
        void LogInformation(string message);

        /// <summary>
        /// Logs a success message.
        /// </summary>
        /// <param name="message">message</param>
        void LogSuccess(string message);

        /// <summary>
        /// Logs a warning message.
        /// </summary>
        /// <param name="message">message</param>
        void LogWarning(string message);

        /// <summary>
        /// Logs a warning message with additional data.
        /// </summary>
        /// <param name="message">message</param>
        /// <param name="exception">exception</param>
        void LogError(string message, Exception exception);
    }
}
