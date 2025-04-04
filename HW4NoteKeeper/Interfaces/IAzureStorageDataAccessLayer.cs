using HW4NoteKeeper.Enums;
using HW4NoteKeeper.Models;
using Microsoft.AspNetCore.Mvc;

namespace HW4NoteKeeper.Interfaces
{
    public interface IAzureStorageDataAccessLayer
    {
        Task<FileStreamResult?> GetAttachment(Guid noteId, string attachmentId);
        
        Task<IEnumerable<AttachmentDetails>> GetNoteAttachmentDetails(Guid noteId);

        Task<BlobStorageResponseUpdateCreate> PutAttachment(DtoAttachment dtoAttachment);

        Task<BlobStorageResponseDelete> DeleteAttachment(Guid noteId, string attachmentId);

        Task<BlobStorageResponseDelete> DeleteAttachmentContainer(Guid noteId);

        int GetMaxAttachments();
    }
}
