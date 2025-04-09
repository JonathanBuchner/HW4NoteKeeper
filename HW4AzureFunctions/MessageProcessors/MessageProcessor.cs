using Azure.Storage.Blobs;
using HW4AzureFunctions.Interfaces;
using HW4NoteKeeper.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO.Compression;
using System.Linq;
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
            var destinationContainer = GetBlobContainerDestination(message.ZipFileId);

            // Get blobs


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

        private async Task<ZipFile> CreateZipFile(BlobContainerClient source, BlobContainerClient destination, List<string> blobNames)
        {
            using var zipStream = new MemoryStream();


        }
    }
}
