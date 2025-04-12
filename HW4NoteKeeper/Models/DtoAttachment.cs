using Newtonsoft.Json;
using NJsonSchema.Annotations;

namespace HW4NoteKeeperEx2.Models
{
    /// <summary>
    /// Data transfer object for attachments.
    /// </summary>
    public class DtoAttachment
    {
        /// <summary>
        /// The note id of the note.  Constraints: Required, Must Exist
        /// </summary>
        [JsonSchemaType(typeof(string))]
        [JsonProperty("noteId")]
        public Guid? NoteId { get; set; }

        /// <summary>
        /// The attachmentId of the attachment
        /// The Id of the attachment which is the name of the file uploaded.
        /// Constraints: Required, Must Exist.
        /// </summary>
        [JsonSchemaType(typeof(string))]
        [JsonProperty("attachmentId")]
        public string AttachmentId { get; set; } = "";

        /// <summary>
        /// The attachment.
        /// </summary>
        [JsonProperty("fileData")]
        public IFormFile? FileData { get; set; }
    }
}
