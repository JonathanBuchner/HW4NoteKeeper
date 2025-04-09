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
    /// <summary>
    /// Azure Storage Data Access Layer.  This class is used to access the Azure Storage Blob service and perform CRUD operations on blobs.
    /// </summary>
    public class AzureStorageDataAccessLayer : IAzureStorageDataAccessLayer
    {
        /// <summary>
        /// Settings for the Azure Storage Data Access Layer.
        /// </summary>
        private readonly AzureStorageDataAccessLayerSettings _settings;

        /// <summary>
        /// Blob service client for the Azure Storage Data Access Layer.
        /// </summary>
        private readonly BlobServiceClient _blobServiceClient;

        public AzureStorageDataAccessLayer(AzureStorageDataAccessLayerSettings settings)
        {
            _settings = settings;
            _blobServiceClient = new BlobServiceClient(settings.ConnectionString);
        }

        /// <summary>
        /// Get an attachment from blob storage.
        /// </summary>
        /// <param name="noteId">note id</param>
        /// <param name="attachmentId">attachment id</param>
        /// <returns>file stream</returns>
        public async Task<FileStreamResult?> GetAttachment(Guid noteId, string attachmentId)
        {
            var containerClient = await GetContainer(noteId.ToString());
            var blobClient = containerClient.GetBlobClient(attachmentId);

            // Return null if blob client does not exist
            if (!await CheckIfBlobExists(noteId, attachmentId))
            {
                return null;
            }

            var blobDownloadInfo = await blobClient.DownloadAsync();
            var contentType = blobDownloadInfo.Value.ContentType;
            var fileStream = blobDownloadInfo.Value.Content; 

            return new FileStreamResult(fileStream, contentType)
            {
                FileDownloadName = attachmentId
            };
        }

        /// <summary>
        /// Get all attachment information for a note.
        /// </summary>
        /// <param name="noteId">Note id</param>
        /// <returns>Attachment details for each note</returns>
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

        /// <summary>
        /// Put an attachment to blob storage.  This method will create a new blob if it does not exist, or update the existing blob if it does exist.
        /// </summary>
        /// <param name="attachment">Attachment</param>
        /// <returns></returns>
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

        /// <summary>
        /// Put an attachment to blob storage.  This method will create a new blob if it does not exist, or update the existing blob if it does exist.
        /// </summary>
        /// <param name="dtoAttachment">attachment</param>
        /// <returns></returns>
        public async Task<BlobStorageResponseUpdateCreate> PutAttachment(DtoAttachment dtoAttachment)
        {
            var attachment = AttachmentValidator.GetAttachmentFromDto(dtoAttachment);

            return await PutAttachment(attachment);
        }

        /// <summary>
        /// Delete an attachment from blob storage.  This method will delete the blob only if it exists.
        /// </summary>
        /// <param name="noteId">Note id</param>
        /// <param name="attachmentId">Attachment id</param>
        /// <returns></returns>
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

        /// <summary>
        /// Delete an attachment container from blob storage.  This method will delete the container only if it exists.
        /// </summary>
        /// <param name="noteId"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Get the maximum number of attachments allowed in a container.
        /// </summary>
        /// <returns></returns>
        public int GetMaxAttachments()
        {
            return _settings.MaxAttachments;
        }

        /// <summary>
        /// Check if a blob exists in the container.  This method will return true if the blob exists, false if it does not exist.
        /// </summary>
        /// <param name="noteId">noteId</param>
        /// <param name="blobId">blobId</param>
        /// <returns>returns true if blob exists</returns>
        public async Task<bool> CheckIfBlobExists(Guid noteId, string blobId)
        {
            var containerClient = await GetContainer(noteId.ToString());
            var blobClient = containerClient.GetBlobClient(blobId);

            if (await blobClient.ExistsAsync())
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Check if the maximum number of blobs has been exceeded in the container.
        /// </summary>
        /// <param name="containerClient">Blob client</param>
        /// <returns></returns>
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

        /// <summary>
        /// Get a container for the blob client.  This method will create a new container if it does not exist.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Upload a file to blob storage.  This method will create a new blob if it does not exist, or update the existing blob if it does exist.
        /// </summary>
        /// <param name="blobClient"></param>
        /// <param name="file"></param>
        /// <returns></returns>
        private async Task UploadFile(BlobClient blobClient, IFormFile file)
        {
            using Stream fileStream = file.OpenReadStream();
            
            await blobClient.UploadAsync(fileStream, new BlobHttpHeaders() { ContentType = file.ContentType });
        }

        /// <summary>
        /// Get metadata for the uploading of a blob.
        /// </summary>
        /// <param name="blobClient"></param>
        /// <param name="existed"></param>
        /// <param name="attachment"></param>
        /// <returns></returns>
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
