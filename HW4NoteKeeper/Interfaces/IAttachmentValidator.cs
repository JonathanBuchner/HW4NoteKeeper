using HW4NoteKeeperEx2.Models;

namespace HW4NoteKeeperEx2.Interfaces
{
    /// <summary>
    /// Interface for validating attachments.
    /// </summary>
    public interface IAttachmentValidator
    {
        /// <summary>
        /// Validates the attachment DTO.
        /// </summary>
        /// <param name="attachment">attachment</param>
        /// <returns>List of errors</returns>
        List<string> ValidateDto(DtoAttachment attachment);
    }
}
