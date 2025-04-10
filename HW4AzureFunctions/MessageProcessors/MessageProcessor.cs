using Azure.Storage.Blobs;
using HW4AzureFunctions.Interfaces;
using HW4NoteKeeper.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO.Compression;
using System.Linq;
using System.Security.Cryptography.Xml;
using System.Text;
using System.Threading.Tasks;

namespace HW4AzureFunctions.MessageProcessors
{
    public class MessageProcessor : IMessageProcessor
    {
        private readonly ILogger<MessageProcessor> _logger;
        private readonly BlobServiceClient _blobServiceClient;

        public MessageProcessor(ILogger<MessageProcessor> logger, BlobServiceClient blobServiceClient)
        {
            _logger = logger;
            _blobServiceClient = blobServiceClient;
        }

        public async Task Process(ZipQueueMessage message)
        {
            LogMessageReceived(message);

            // Get source container
            var sourceContainer = GetBlobContainerSource(message.NoteId);

            if (sourceContainer == null)
            {
                return;
            }

            // Get blobs in source container
            var blobNames = await GetBlobNamesInContainer(sourceContainer);

            if (blobNames == null || blobNames.Count == 0)
            {
                return;
            }

            // Get destination container
            var destinationContainer = await GetBlobContainerDestination(message.ZipFileId);

            // Get zip memmory stream
            using var zipStream = await CreateZipMemoryStream(sourceContainer, blobNames);

            // Upload the zip file to the destination container
            await UploadZipToDestination(destinationContainer, zipStream, message.ZipFileId);

            _logger.LogInformation($"Zip file {message.ZipFileId} created and uploaded successfully.");
        }
        private void LogMessageReceived(ZipQueueMessage message)
        {
            _logger.LogInformation("Processing message: {Message}", message);
        }

        private BlobContainerClient GetBlobContainerSource(Guid noteId)
        {
            return _blobServiceClient.GetBlobContainerClient(noteId.ToString());
        }

        private async Task<List<string>> GetBlobNamesInContainer(BlobContainerClient sourceContainer)
        {
            var blobNames = new List<string>();

            await foreach (var blobItem in sourceContainer.GetBlobsAsync())
            {
                blobNames.Add(blobItem.Name);
            }

            return blobNames;
        }

        private async Task<BlobContainerClient> GetBlobContainerDestination(string zipFileId)
        {
            var container = _blobServiceClient.GetBlobContainerClient(zipFileId);

            await container.CreateIfNotExistsAsync();

            return container;
        }

        private async Task<MemoryStream> CreateZipMemoryStream(BlobContainerClient source, List<string> blobNames)
        {
            using var zipStream = new MemoryStream();

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

        private async Task UploadZipToDestination(BlobContainerClient destinationContainer, MemoryStream zipStream, string zipFileName)
        {
            var blobClient = destinationContainer.GetBlobClient(zipFileName);

            await blobClient.UploadAsync(zipStream, true);
        }
    }
}
