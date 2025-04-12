using System.IO.Compression;
using HW4NoteKeeperWebJobTelemetry;
using HW4NoteKeeperWebJob.Models;

using HW4NoteKeeperWebJob.Interfaces;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

namespace HW4NoteKeeperWebJob.MessageProcessors
{
    /// <summary>
    /// Class for processing messages from the zip request queue.
    /// </summary>
    public class MessageProcessor : IMessageProcessor
    {
        /// <summary>
        /// Blob service client for accessing Azure Blob Storage.
        /// </summary>
        private readonly BlobServiceClient _blobServiceClient;

        /// <summary>
        /// Telemetry client for logging.
        /// </summary>
        private readonly ITelemetry _telemetry;

        public MessageProcessor(BlobServiceClient blobServiceClient, ITelemetry telemetry)
        {
            _blobServiceClient = blobServiceClient;
            _telemetry = telemetry;
        }

        /// <summary>
        /// Processes a message from the zip request queue.
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public async Task Process(ZipQueueMessage message)
        {
            try
            { 
                LogMessageReceived(message);

                // Get source container
                var sourceContainer = GetBlobContainerSource(message.NoteId);

                if (sourceContainer == null)
                {
                    return;
                }

                // Get blobs names source container
                var blobNames = await GetBlobNamesInContainer(sourceContainer);

                if (blobNames == null || blobNames.Count == 0)
                {
                    return;
                }

                // Get destination container
                var destinationContainer = GetBlobContainerDestination(message.NoteId);

                // Get zip memmory stream
                using var zipStream = await CreateZipMemoryStream(sourceContainer, blobNames);

                // Upload the zip file to the destination container
                await UploadZipToDestination(destinationContainer, zipStream, message.ZipFileId);

                LogSuccess(message);
            }
            catch (Exception ex)
            {
                LogException(ex);

                throw;
            }
        }

        /// <summary>
        /// Log message received from the queue.
        /// </summary>
        /// <param name="message">message</param>
        private void LogMessageReceived(ZipQueueMessage message)
        {
            _telemetry.LogInformation($"Processing message: {message.NoteId}, {message.ZipFileId}");
        }

        /// <summary>
        /// Log exception.
        /// </summary>
        /// <param name="ex">exception</param>
        private void LogException(Exception ex)
        {
            _telemetry.LogError("An error occurred while processing the message.", ex);
        }

        /// <summary>
        /// Log success message.
        /// </summary>
        /// <param name="message">message</param>
        private void LogSuccess(ZipQueueMessage message)
        {
            _telemetry.LogSuccess($"Zip file {message.ZipFileId} created and uploaded successfully.");
        }

        /// <summary>
        /// Get the blob container client for the source container.
        /// </summary>
        /// <param name="noteId"></param>
        /// <returns>source blob container</returns>
        private BlobContainerClient GetBlobContainerSource(Guid noteId)
        {
            return _blobServiceClient.GetBlobContainerClient(noteId.ToString());
        }

        /// <summary>
        /// Get the blob container client for the destination container.
        /// </summary>
        /// <param name="noteId">note id</param>
        /// <returns>destination blob container</returns>
        private BlobContainerClient GetBlobContainerDestination(Guid noteId)
        {
            var name = $"{noteId}-zip";

            var container = _blobServiceClient.GetBlobContainerClient(name);

            if (!container.Exists())
            {
                container.Create();
            }

            return container;
        }

        /// <summary>
        /// Get the names of all blobs in the source container.
        /// </summary>
        /// <param name="sourceContainer">Blob container</param>
        /// <returns>blob names</returns>
        private async Task<List<string>> GetBlobNamesInContainer(BlobContainerClient sourceContainer)
        {
            var blobNames = new List<string>();

            await foreach (var blobItem in sourceContainer.GetBlobsAsync())
            {
                blobNames.Add(blobItem.Name);
            }

            return blobNames;
        }

        /// <summary>
        /// Create a zip memory stream from the blobs.
        /// </summary>
        /// <param name="source">blobcontainer</param>
        /// <param name="blobNames">blobnames</param>
        /// <returns>Memory stream</returns>
        private async Task<MemoryStream> CreateZipMemoryStream(BlobContainerClient source, List<string> blobNames)
        {
            var zipStream = new MemoryStream();

            // True indicates that the stream should be left open after the ZipArchive is disposed.
            using (var zip = new ZipArchive(zipStream, ZipArchiveMode.Create, true))
            {
                foreach (var blobName in blobNames)
                {
                    // Get the blob client and open a stream to read the blob
                    var blobClient = source.GetBlobClient(blobName);
                    var blobStream = await blobClient.OpenReadAsync();

                    // Create a new entry in the zip file
                    var zipEntry = zip.CreateEntry(blobName);

                    // Copy the blob stream to the zip entry
                    using (var entryStream = zipEntry.Open())
                    {
                        await blobStream.CopyToAsync(entryStream);
                    }
                }
            }

            // Reset the stream position to the beginning
            zipStream.Position = 0;

            return zipStream;
        }

        /// <summary>
        /// Upload the zip file to the destination container.
        /// </summary>
        /// <param name="destinationContainer">blob client</param>
        /// <param name="zipStream">zip stream</param>
        /// <param name="zipFileName">file name</param>
        /// <returns></returns>
        private async Task UploadZipToDestination(BlobContainerClient destinationContainer, MemoryStream zipStream, string zipFileName)
        {
            var blobClient = destinationContainer.GetBlobClient(zipFileName);

            var headers = new BlobHttpHeaders
            {
                ContentType = "application/zip"
            };
            zipStream.Position = 0;

            // I looked this up on ChatGPT to get the content type correct.
            await blobClient.UploadAsync(
                content: zipStream,
                options: new BlobUploadOptions
                {
                    HttpHeaders = new BlobHttpHeaders
                    {
                        ContentType = "application/zip"
                    }
                }
            );

            var metaData = GetMetaDataForZipUploadedBlob(zipStream, zipFileName);
            await blobClient.SetMetadataAsync(metaData);
        }

        /// <summary>
        /// Get metadata for the uploading of a zip blob.
        /// </summary>
        /// <param name="zipFileName">zip file name</param>
        /// <param name="zipStream">zip stream</param>
        /// <returns></returns>
        private Dictionary<string, string> GetMetaDataForZipUploadedBlob(MemoryStream zipStream, string zipFileName)
        {
            var metaData = new Dictionary<string, string>() { };

            metaData.Add("zipFileId", zipFileName);
            metaData.Add("Created", DateTime.UtcNow.ToString("o"));
            metaData.Add("LastModified", DateTime.UtcNow.ToString("o"));
            metaData.Add("ContentType", "application/zip");
            metaData.Add("Length", zipStream.Length.ToString());

            return metaData;
        }
    }
}
