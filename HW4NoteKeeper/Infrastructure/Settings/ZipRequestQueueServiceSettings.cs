namespace HW4NoteKeeperEx2.Infrastructure.Settings
{
    /// <summary>
    /// Settings for the ZipRequestService.
    /// </summary>
    public class ZipRequestQueueServiceSettings
    {
        /// <summary>
        /// The connection string to the Azure Storage Queue.
        /// </summary>
        public string ConnectionString { get; set; } = String.Empty;

        /// <summary>
        /// The name of the Azure Storage Queue.
        /// </summary>
        public string QueueNameFunction { get; set; } = String.Empty;

        /// <summary>
        /// The name of the WebJob queueue.
        /// </summary>
        public string QueueNameWebJob { get; set; } = String.Empty;
    }
}
