using HW4NoteKeeperEx2.Enums;
using HW4NoteKeeperEx2.Models;
using Microsoft.AspNetCore.Mvc;

namespace HW4NoteKeeperEx2.Interfaces
{
    /// <summary>
    /// Interface for Azure Storage Data Access Layer.
    /// </summary>
    public interface IAzureStorageDataAccessLayer
    {
        /// <summary>
        /// Get attachment from storage.
        /// </summary>
        /// <param name="noteId">Note id</param>
        /// <param name="attachmentId">Attachment id</param>
        /// <returns></returns>
        Task<FileStreamResult?> GetAttachment(Guid noteId, string attachmentId);

        /// <summary>
        /// Get attachment from storage.
        /// </summary>
        /// <param name="noteId">Note id</param>
        /// <param name="attachmentId">Attachment id</param>
        /// <returns></returns>
        Task<FileStreamResult?> GetAttachment(String noteId, string attachmentId);

        /// <summary>
        /// Get all details for each attachment attached to a note.
        /// </summary>
        /// <param name="noteId">note id</param>
        /// <returns>list of attachment details</returns>
        Task<IEnumerable<AttachmentDetails>> GetNoteAttachmentDetails(Guid noteId);

        /// <summary>
        /// Get all details for each attachment attached to a note.
        /// </summary>
        /// <param name="noteId">note id</param>
        /// <returns>list of attachment details</returns>
        Task<IEnumerable<DtoZipAttachmentDetails>> GetNoteZipAttachmentDetails(string noteId);

        /// <summary>
        /// Upload an attachment to Azure storage.
        /// </summary>
        /// <param name="attachment">attachment</param>
        /// <returns>Blob storage message</returns>
        Task<BlobStorageResponseUpdateCreate> PutAttachment(Attachment attachment);

        /// <summary>
        /// Upload an attachment to Azure storage.
        /// </summary>
        /// <param name="attachment">attachment</param>
        /// <returns>Blob storage message</returns>
        Task<BlobStorageResponseUpdateCreate> CreateZipAttachment(Attachment attachment);

        /// <summary>
        /// Upload an attachment to Azure storage.
        /// </summary>
        /// <param name="dtoAttachment">attachment</param>
        /// <returns>Blob storage message</returns>
        Task<BlobStorageResponseUpdateCreate> PutAttachment(DtoAttachment dtoAttachment);

        /// <summary>
        /// Delete an attachment from Azure storage.
        /// </summary>
        /// <param name="noteId">note id</param>
        /// <param name="attachmentId">attachmet id</param>
        /// <returns>Blob storage messages</returns>
        Task<BlobStorageResponseDelete> DeleteAttachment(Guid noteId, string attachmentId);


        /// <summary>
        /// Delete an attachment from Azure storage.
        /// </summary>
        /// <param name="noteId"></param>
        /// <param name="attachmentId"></param>
        /// <returns></returns>
        Task<BlobStorageResponseDelete> DeleteAttachment(string noteId, string attachmentId);

        /// <summary>
        /// Delete all attachments for a note.
        /// </summary>
        /// <param name="noteId">Note id</param>
        /// <returns>Delete container for a note id</returns>
        Task<BlobStorageResponseDelete> DeleteAttachmentContainer(Guid noteId);

        /// <summary>
        /// Delete all attachments for a note.
        /// </summary>
        /// <param name="noteId">Note id</param>
        /// <returns>Delete container for a note id</returns>
        Task<BlobStorageResponseDelete> DeleteAttachmentContainer(string noteId);

        /// <summary>
        /// Get the maximum number of attachments allowed for a note.
        /// </summary>
        /// <returns></returns>
        int GetMaxAttachments();

        /// <summary>
        /// Check if a blob exists in the storage.
        /// </summary>
        /// <param name="noteId">note id</param>
        /// <param name="blobId">blob id</param>
        /// <returns>returns true if blob exists</returns>
        Task<bool> CheckIfBlobExists(Guid noteId, string blobId);

        /// <summary>
        /// Check if a blob exists in the storage.
        /// </summary>
        /// <param name="noteId">note id</param>
        /// <param name="blobId">blob id</param>
        /// <returns>returns true if blob exists</returns>
        Task<bool> CheckIfBlobExists(string noteId, string blobId);
    }
}
