using Azure.Storage.Queues;
using System.Text.Json;
using HW4NoteKeeperEx2.Infrastructure.Settings;
using HW4NoteKeeperEx2.Interfaces;
using System.Threading.Tasks;
using HW4NoteKeeperEx2.Models;

namespace HW4NoteKeeperEx2.DataAccessLayer
{
    /// <summary>
    /// Service for managing zip request queues.
    /// </summary>
    public class ZipRequestQueueService : IZipRequestQueueService
    {
        /// <summary>
        /// The Azure Storage Queue client.
        /// </summary>
        private readonly QueueClient _queueClientFunction;

        /// <summary>
        /// The Azure Storage Queue client.
        /// </summary>
        private readonly QueueClient _queueClientWebJob;

        public ZipRequestQueueService(ZipRequestQueueServiceSettings settings)
        {
            _queueClientFunction = new QueueClient(settings.ConnectionString, settings.QueueNameFunction);
            _queueClientFunction.CreateIfNotExists();

            _queueClientWebJob = new QueueClient(settings.ConnectionString, settings.QueueNameWebJob);
            _queueClientFunction.CreateIfNotExists();
        }

        /// <summary>
        /// Enqueues a zip request message to the Azure Storage Queue.
        /// </summary>
        /// <param name="noteId">note id</param>
        /// <param name="zipFileName">zip file name</param>
        /// <returns>Task</returns>
        public async Task EnqueueAsync(Guid noteId, string zipFileName, string queueName)
        {
            var message = new ZipQueueMessage
            {
                NoteId = noteId,
                ZipFileId = zipFileName
            };

            var messageBody = JsonSerializer.Serialize(message);


            if (queueName == _queueClientWebJob.Name)
            {
                await _queueClientWebJob.SendMessageAsync(messageBody);
            }
            else
            {
                await _queueClientFunction.SendMessageAsync(messageBody);
            }
        }
    }
}


