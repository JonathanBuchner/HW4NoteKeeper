using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs.Specialized;
using HW4NoteKeeper.ApplicationInsightsTrackers;
using HW4NoteKeeper.Enums;
using HW4NoteKeeper.Infrastructure.Settings;
using HW4NoteKeeper.Interfaces;
using HW4NoteKeeper.Models;
using HW4NoteKeeper.Validators;
using Microsoft.AspNetCore.Mvc;

namespace HW4NoteKeeper.DataAccessLayer
{
    public class AzureStorageDataAccessLayer : IAzureStorageDataAccessLayer
    {
        private readonly AzureStorageDataAccessLayerSettings _settings;
        private readonly BlobServiceClient _blobServiceClient;

        public AzureStorageDataAccessLayer(AzureStorageDataAccessLayerSettings settings)
        {
            _settings = settings;
            _blobServiceClient = new BlobServiceClient(settings.ConnectionString);
        }

        public async Task<FileStreamResult?> GetAttachment(Guid noteId, string attachmentId)
        {
            var containerClient = await GetContainer(noteId.ToString());
            var blobClient = containerClient.GetBlobClient(attachmentId);

            // Return null if blob client does not exist
            if (!await blobClient.ExistsAsync())
            {
                return null;
            }

            //
            var blobDownloadInfo = await blobClient.DownloadAsync();
            var contentType = blobDownloadInfo.Value.ContentType;
            var fileStream = blobDownloadInfo.Value.Content; 

            return new FileStreamResult(fileStream, contentType)
            {
                FileDownloadName = attachmentId
            };
        }

        public async Task<IEnumerable<AttachmentDetails>> GetNoteAttachmentDetails(Guid noteId)
        {
            var containerClient = await GetContainer(noteId.ToString());
            var attachmentDetailsList = new List<AttachmentDetails>();

            await foreach (var blobItem in containerClient.GetBlobsAsync(traits: BlobTraits.Metadata))
            {
                // Retrieve metadata 
                var name = blobItem.Name;

                blobItem.Metadata.TryGetValue("Created", out var createdMetadata);
                var createdDate = DateTimeOffset.TryParse(createdMetadata, out var created)
                    ? created
                    : blobItem.Properties.LastModified ?? DateTimeOffset.MinValue;


                var lastModifiedDate = blobItem.Properties.LastModified ?? DateTimeOffset.MinValue;
                var contentType = blobItem.Properties.ContentType ?? "application/octet-stream";    // Default to binary
                var length = (blobItem.Properties.ContentLength ?? 0).ToString();                   

                var attachmentDetails = new AttachmentDetails
                {
                    AttachmentId = name,
                    ContentType = contentType,
                    CreatedDate = createdDate,           
                    LastModifiedDate = lastModifiedDate, 
                    Length = length                      
                };

                attachmentDetailsList.Add(attachmentDetails);
            }

            return attachmentDetailsList;
        }

        public async Task<BlobStorageResponseUpdateCreate> PutAttachment(Attachment attachment)
        {
            // Create a container if it doesn't exist
            var containerClient = await GetContainer(attachment.NoteId.ToString());

            // If there there has exceeded to many files in the container, return rejected response. Check for the amount of attachments in the container. Stop if the limit is reached. Counting pas pages may have performance improvments
            if (await IsMaxBlobsExceeded(containerClient))
            {
                return BlobStorageResponseUpdateCreate.RejectedMaxSizeExceeded;
            }

            BlobClient blobClient = containerClient.GetBlobClient(attachment.AttachmentId.ToString());
            var blobClientAlreaydExists = await blobClient.ExistsAsync();

            // Upload the file to the container. Will overwrite if the file already exists.
            await UploadFile(blobClient, attachment.FileData);

            // Set metadata for the blob
            var metaData = await GetMetaDataForUploadedBlob(blobClient, blobClientAlreaydExists, attachment);
            await blobClient.SetMetadataAsync(metaData);

            if (blobClientAlreaydExists)
            {
                return BlobStorageResponseUpdateCreate.Updated;
            }

            return BlobStorageResponseUpdateCreate.Created;
        }

        public async Task<BlobStorageResponseUpdateCreate> PutAttachment(DtoAttachment dtoAttachment)
        {
            var attachment = AttachmentValidator.GetAttachmentFromDto(dtoAttachment);

            return await PutAttachment(attachment);
        }

        public async Task<BlobStorageResponseDelete> DeleteAttachment(Guid noteId, string attachmentId)
        {

            var containerClient = await GetContainer(noteId.ToString());
            var blobClient = containerClient.GetBlobClient(attachmentId);

            if (!await blobClient.ExistsAsync())
            {
                return BlobStorageResponseDelete.NotFound;
            }

            await blobClient.DeleteIfExistsAsync();

            return BlobStorageResponseDelete.Deleted;
        }

        public async Task<BlobStorageResponseDelete> DeleteAttachmentContainer(Guid noteId)
        {
            var containerClient = CreateBlobContainerClient(noteId.ToString());

            if (!await containerClient.ExistsAsync())
            {
                return BlobStorageResponseDelete.NotFound;
            }

            await containerClient.DeleteAsync();

            return BlobStorageResponseDelete.Deleted;
        }

        /// <summary>
        /// Create a blob service client.
        /// </summary>
        /// <param name="containerName">container name for client.</param>
        /// <returns>client</returns>
        public BlobContainerClient CreateBlobContainerClient(string containerName)
        {
            return new BlobContainerClient(_settings.ConnectionString, containerName);
        }

        public int GetMaxAttachments()
        {
            return _settings.MaxAttachments;
        }

        private async Task<bool> IsMaxBlobsExceeded(BlobContainerClient containerClient)
        {
            var blobCount = 0;
            
            await foreach (var page in containerClient.GetBlobsAsync(traits: BlobTraits.None).AsPages(pageSizeHint: GetMaxAttachments()))
            {
                blobCount += page.Values.Count;

                // Stop early if we reach the threshold
                if (blobCount >= GetMaxAttachments())
                    return true;
            }

            return false;
        }

        private async Task<BlobContainerClient> GetContainer(string name)
        {
            var containerClient = CreateBlobContainerClient(name);

            if (!await containerClient.ExistsAsync())
            {
                await containerClient.CreateIfNotExistsAsync();
                await containerClient.SetAccessPolicyAsync(PublicAccessType.None);                  // Set the access policy to private
            }

            return containerClient;
        }

        private async Task UploadFile(BlobClient blobClient, IFormFile file)
        {
            using Stream fileStream = file.OpenReadStream();
            
            await blobClient.UploadAsync(fileStream, new BlobHttpHeaders() { ContentType = file.ContentType });
        }

        private async Task<Dictionary<string, string>> GetMetaDataForUploadedBlob(BlobClient blobClient, bool existed, Attachment attachment)
        {
            var metaData = new Dictionary<string, string>
            {
                { "NoteId", attachment.NoteId.ToString() },
                { "AttachmentId", attachment.AttachmentId.ToString() },     // AttachmentId is tracked but it should match the blob name which is already retrievable.
            };

            if (existed)
            {
                var existingMetadata = await blobClient.GetPropertiesAsync();

                if (existingMetadata.Value.Metadata.TryGetValue("Created", out var existingCreated))
                {
                    metaData.Add("Created", existingCreated);
                }
                // Note that is is being set, but this is already tracked by the blob service
                metaData.Add("Modified", DateTime.UtcNow.ToString("o"));
            }
            else
            { 
                metaData.Add("Created", DateTime.UtcNow.ToString("o"));
            }

            return metaData;
        }
    }
}
