namespace HW4NoteKeeper.Interfaces
{
    /// <summary>
    /// Interface for application insights exception tracker.
    /// </summary>
    /// <typeparam name="T">Payload to log</typeparam>
    public interface IApplicationLogger<T>
    {
        /// <summary>
        /// Track exception through application insights. Also logs exception to logger.
        /// </summary>
        /// <param name="ex">Exception to log</param>
        void LogException(Exception ex);

        /// <summary>
        /// Track exception through application insights. Also logs exception to logger.
        /// </summary>
        /// <param name="ex">Exception to log</param>
        /// <param name="payload">Payload to log</param>
        void LogException(Exception ex, T payload);

        /// <summary>
        /// Log warning through application insights. Also logs exception to logger.
        /// </summary>
        /// <param name="WarningMessage">Message with the warning</param>
        void LogWarning(string WarningMessage);

        /// <summary>
        /// Log warning through application insights. Also logs exception to logger.
        /// </summary>
        /// <param name="WarningMessage">Message with the warning</param>
        /// <param name="additionalData">Additional data to log</param>
        void LogWarning(string WarningMessage, Dictionary<string, object> additionalData);

        /// <summary>
        /// Log warning through application insights. Also logs exception to logger.
        /// </summary>
        /// <param name="WarningMessage">Message with the warning</param>
        /// <param name="payload">Object that caused warning.</param>
        void LogWarning(string WarningMessage, T payload);

        /// <summary>
        /// Log warning through application insights. Also logs exception to logger.
        /// </summary>
        /// <param name="WarningMessage">Message with the warning</param>
        /// <param name="payload">Object that caused warning.</param>
        /// <param name="additionalData">Additional data to log</param>
        void LogWarning(string WarningMessage, T payload, Dictionary<string, object> additionalData);

        /// <summary>
        /// Log information through application insights. Also logs information to logger.
        /// </summary>
        /// <param name="message">Message to log</param>
        void LogInformation(string message);

        /// <summary>
        /// Log information through application insights. Also logs information to logger.
        /// </summary>
        /// <param name="message">Message to log</param>
        /// <param name="additionalData">Additional information to log</param>
        void LogInformation(string message, Dictionary<string, object> additionalData);

        /// <summary>
        /// Log information through application insights. Also logs information to logger.
        /// </summary>
        /// <param name="message">Message to log</param>
        /// <param name="payload">payload</param>
        void LogInformation(string message, T payload);

        /// <summary>
        /// Log information through application insights. Also logs information to logger.
        /// </summary>
        /// <param name="message">Message to log</param>
        /// <param name="payload">payload</param>
        /// <param name="additionalData">Additional information to log</param>
        void LogInformation(string message, T payload, Dictionary<string, object> additionalData);
    }
}
