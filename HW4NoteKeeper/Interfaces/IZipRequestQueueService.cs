namespace HW4NoteKeeperEx2.Interfaces
{
    /// <summary>
    /// Zip request service interface. Makes requests to the Azure Storage Queue for zipping attachments.
    /// </summary>
    public interface IZipRequestQueueService
    {
        /// <summary>
        /// Enqueues a zip request message to the Azure Storage Queue for zipping attachment.
        /// </summary>
        /// <param name="noteId">note id</param>
        /// <param name="zipFileName">zip file name</param>
        /// <param name="queueName">queue name</param>
        /// <returns>Task</returns>
        Task EnqueueAsync(Guid noteId, string zipFileName, string queueName);
    }
}
