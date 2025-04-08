using HW4NoteKeeper.Models;

namespace HW4NoteKeeper.Interfaces
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
