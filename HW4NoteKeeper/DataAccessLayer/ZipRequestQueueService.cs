using Azure.Storage.Queues;
using System.Text.Json;
using HW4NoteKeeper.Infrastructure.Settings;
using HW4NoteKeeper.Interfaces;
using System.Threading.Tasks;
using HW4NoteKeeper.Models;

namespace HW4NoteKeeper.DataAccessLayer
{
    /// <summary>
    /// Service for managing zip request queues.
    /// </summary>
    public class ZipRequestQueueService : IZipRequestQueueService
    {
        /// <summary>
        /// The Azure Storage Queue client.
        /// </summary>
        private readonly QueueClient _queueClient;

        public ZipRequestQueueService(ZipRequestQueueServiceSettings settings)
        {
            _queueClient = new QueueClient(settings.ConnectionString, settings.QueueName);
            _queueClient.CreateIfNotExists();
        }

        /// <summary>
        /// Enqueues a zip request message to the Azure Storage Queue.
        /// </summary>
        /// <param name="noteId">note id</param>
        /// <param name="zipFileName">zip file name</param>
        /// <returns>Task</returns>
        public async Task EnqueueAsync(Guid noteId, string zipFileName)
        {
            var message = new ZipQueueMessage
            {
                NoteId = noteId,
                ZipFileId = zipFileName
            };

            var messageBody = JsonSerializer.Serialize(message);

            await _queueClient.SendMessageAsync(messageBody);
        }
    }
}


