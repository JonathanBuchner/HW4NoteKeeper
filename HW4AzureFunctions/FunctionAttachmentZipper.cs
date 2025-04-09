using Azure.Storage.Blobs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace HW4AzureFunctions
{
    public class FunctionAttachmentZipper
    {
        private readonly ILogger<FunctionAttachmentZipper> _logger;
        private readonly BlobServiceClient _blobServiceClient;

        public FunctionAttachmentZipper(ILogger<FunctionAttachmentZipper> logger, BlobServiceClient blobServiceClient)
        {
            _logger = logger;
            _blobServiceClient = blobServiceClient;
        }
    }
}
