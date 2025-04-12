using Azure.Storage.Queues;
using HW4AzureFunctions.Interfaces;
using HW4AzureFunctions.Models;
using HW4AzureFunctions.Telemetry;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace HW4AzureFunctions
{
    /// <summary>
    /// Function that zips attachments for notes.  This function is triggered by a timer and checks the queue for messages to process.
    /// </summary>
    public class FunctionAttachmentZipperTimer
    {
        /// <summary>
        /// Telemetry instance for logging.
        /// </summary>
        private readonly ITelemetry _telemetry;

        /// <summary>
        /// Message handler instance for processing messages.
        /// </summary>
        private readonly IMessageProcessor _messageHandler;

        /// <summary>
        /// Queue client instance for accessing Azure Queue Storage.
        /// </summary>
        private readonly QueueClient _queueClient;

        /// <summary>
        /// Name of the queue to process messages from.
        /// </summary>
        private const string QueueName = "attachment-zip-requests";

        public FunctionAttachmentZipperTimer(
            ITelemetry telemetry,
            IMessageProcessor messageProcessor,
            IConfiguration config)
        {
            _telemetry = telemetry;
            _messageHandler = messageProcessor;
            _queueClient = SetQueueClient(config);
        }

        /// <summary>
        /// Function that zips.  This function is triggered by a timer and checks the queue for messages to process.
        /// </summary>
        /// <param name="timer">timer</param>
        /// <returns>Task</returns>
        [Function("ZipAttachments")]
        public async Task Run([TimerTrigger("*/5 * * * * *")] TimerInfo timer)
        {
            _telemetry.LogInformation("Timer triggered. Checking queue...");

            var response = await _queueClient.ReceiveMessageAsync();

            if (response.Value != null)
            {
                var rawMessage = response.Value.MessageText;
                _telemetry.LogInformation($"Dequeued message: {rawMessage}");

                try
                {
                    var message = JsonConvert.DeserializeObject<ZipQueueMessage>(rawMessage);
                    
                    ValidateQueueMessage(message);

                    if (message == null)
                    {
                        throw new Exception("This exception cannot be hig if validate queue message is working correctly.  It removes warning.");
                    }

                    await _messageHandler.Process(message);
                    
                    await _queueClient.DeleteMessageAsync(response.Value.MessageId, response.Value.PopReceipt);
                }
                catch (Exception ex)
                {
                    _telemetry.LogError("Error while processing queue message.", ex);
                }
            }
            else
            {
                _telemetry.LogInformation("No messages in the queue.");
            }
        }

        /// <summary>
        /// Sets the queue client for accessing Azure Queue Storage.
        /// </summary>
        /// <param name="config">config</param>
        /// <returns>Queue client</returns>
        private QueueClient SetQueueClient(IConfiguration config)
        {
            var connectionString = config["AzureWebJobsStorage"];
            var queueClient = new QueueClient(connectionString, QueueName);
            queueClient.CreateIfNotExists();

            return queueClient;
        }

        /// <summary>
        /// Validates the queue message.
        /// </summary>
        /// <param name="message">message</param>
        /// <exception cref="ArgumentNullException">Thrown if returns as null.</exception>
        /// <exception cref="ArgumentException">Exception thrown if argument is missing.</exception>
        private static void ValidateQueueMessage(ZipQueueMessage? message)
        {
            if (message == null)
            { 
                throw new ArgumentNullException(nameof(message), "Queue message cannot be null.");
            }

            if (message.NoteId == Guid.Empty)
            {
                throw new ArgumentException("NoteId cannot be empty.");
            }

            if (string.IsNullOrWhiteSpace(message.ZipFileId))
            {
                throw new ArgumentException("ZipFileId cannot be null or empty.");
            }
        }
    }
}
