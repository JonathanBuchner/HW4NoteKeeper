namespace HW4NoteKeeperEx2.Infrastructure.Settings
{
    /// <summary>
    /// Appliation insights settigns.
    /// </summary>
    public class ApplicationInsightsSettings
    {
        /// <summary>
        /// API Authnetication key
        /// </summary>
        /// <remarks>Empty key is provided on instantiation which will fail validation.</remarks>
        public string ApiKey { get; set; } = "";

        /// <summary>
        /// Redues the amount of entires logged (reduces expense)
        /// </summary>
        /// <value>True means adaptive sampling is enabled.</value>
        public bool EnableAdaptiveSampling { get; set; } = true;

        /// <summary>
        /// Application insights connection string.
        /// </summary>
        public string ConnectionString { get; set; } = "";
    }
}
