using HW4NoteKeeper.Enums;
using HW4NoteKeeper.Models;
using Microsoft.AspNetCore.Mvc;

namespace HW4NoteKeeper.Interfaces
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
        /// Get all details for each attachment attached to a note.
        /// </summary>
        /// <param name="noteId">note id</param>
        /// <returns>list of attachment details</returns>
        Task<IEnumerable<AttachmentDetails>> GetNoteAttachmentDetails(Guid noteId);

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
        Task<BlobStorageResponseUpdateCreate> PutAttachment(DtoAttachment dtoAttachment);

        /// <summary>
        /// Delete an attachment from Azure storage.
        /// </summary>
        /// <param name="noteId">note id</param>
        /// <param name="attachmentId">attachmet id</param>
        /// <returns>Blob storage messages</returns>
        Task<BlobStorageResponseDelete> DeleteAttachment(Guid noteId, string attachmentId);

        /// <summary>
        /// Delete all attachments for a note.
        /// </summary>
        /// <param name="noteId">Note id</param>
        /// <returns>Delete container for a note id</returns>
        Task<BlobStorageResponseDelete> DeleteAttachmentContainer(Guid noteId);

        /// <summary>
        /// Get the maximum number of attachments allowed for a note.
        /// </summary>
        /// <returns></returns>
        int GetMaxAttachments();
    }
}
