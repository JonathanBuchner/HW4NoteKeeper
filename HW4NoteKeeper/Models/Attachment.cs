using HW4NoteKeeper.Validators;
using Newtonsoft.Json;
using NJsonSchema.Annotations;

namespace HW4NoteKeeper.Models
{
    /// <summary>
    /// Data transfer object for attachments.
    /// </summary>
    public class Attachment
    {
        /// <summary>
        /// The note id of the note.  Constraints: Required, Must Exist
        /// </summary>
        [JsonSchemaType(typeof(string))]
        [JsonProperty("noteId")]
        public Guid NoteId { get; set; } = Guid.Empty;

        /// <summary>
        /// The attachmentId of the attachment
        /// The Id of the attachment which is the name of the file uploaded.
        /// Constraints: Required, Must Exist.
        /// </summary>
        [JsonSchemaType(typeof(string))]
        [JsonProperty("attachmentId")]
        public string AttachmentId { get; set; } = "";

        [JsonProperty("attachmentId")]
        public required IFormFile FileData { get; set; }
    }
}
