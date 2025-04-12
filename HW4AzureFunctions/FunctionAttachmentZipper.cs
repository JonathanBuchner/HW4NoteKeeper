/*
 * I left this code in the file to contine to test a non-timer. 
 */

/*
using Azure.Storage.Blobs;
using HW4AzureFunctions.Interfaces;
using HW4AzureFunctions.MessageProcessors;
using HW4NoteKeeper.Models;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace HW4AzureFunctions
{
    public class FunctionAttachmentZipper
    {
        private readonly ILogger<FunctionAttachmentZipper> _logger;

        private const string QueueName = "attachment-zip-requests";
        private readonly IMessageProcessor messageHandler;

        public FunctionAttachmentZipper(ILogger<FunctionAttachmentZipper> logger, IMessageProcessor messageProcessor)
        {
            _logger = logger;
            messageHandler = messageProcessor;
        }

        /// <summary>
        /// 
        /// 
        /// Note: Connection = "AzureWebJobsStorage" is used to connect to the Azure Storage account and references the secret.json file.
        /// </summary>
        /// <param name="queueMessage"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        [Function("ZipAttachmentsNotTimer")]
        public async Task Run([QueueTrigger(QueueName, Connection = "AzureWebJobsStorage")] string queueMessage, FunctionContext context)
        {
            LogMessageReceived(queueMessage);

            ZipQueueMessage message;

            // We only log in
            try
            {
                // Use null-coalescing operator to handle potential null value
                ZipQueueMessage? zipQueueMessage = JsonConvert.DeserializeObject<ZipQueueMessage>(queueMessage);

                message = ValidateQueueMessage(zipQueueMessage);

                try
                {
                    // Process the message
                    await messageHandler.Process(message);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "An error occurred while processing the message.");

                    throw;
                }
            }
            catch (ArgumentNullException argEx)
            {
                _logger.LogError(argEx, "Queue message is null or empty.");
            }
            catch (ArgumentException argEx)
            {
                _logger.LogError(argEx, "Invalid argument in queue message.");
            }
            catch (JsonException jsonEx)
            {
                _logger.LogError(jsonEx, "Failed to deserialize the queue message.");

                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while processing the queue message.");

                throw;
            }
        }

        private void LogMessageReceived(string queueMessage)
        {
            _logger.LogInformation("Queue trigger received message: {Message}", queueMessage);
        }

        private ZipQueueMessage ValidateQueueMessage(ZipQueueMessage? message)
        {
            if (message == null)
            {
                throw new ArgumentNullException(nameof(message), "Queue message cannot be null.");
            }

            if (message.NoteId == Guid.Empty)
            {
                throw new ArgumentException("NoteId cannot be empty.", nameof(message.NoteId));
            }

            if (string.IsNullOrWhiteSpace(message.ZipFileId))
            {
                throw new ArgumentException("ZipFileId cannot be null or empty.", nameof(message.ZipFileId));
            }

            return message;
        }
    }
}
*/