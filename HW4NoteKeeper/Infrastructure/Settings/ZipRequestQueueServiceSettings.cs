namespace HW4NoteKeeper.Infrastructure.Settings
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
        public string QueueName { get; set; } = String.Empty;
    }
}
