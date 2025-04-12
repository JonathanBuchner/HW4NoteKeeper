using Newtonsoft.Json;
using NJsonSchema.Annotations;

namespace HW4NoteKeeperEx2.Models
{
    /// <summary>
    /// Represents a message in the zip request queue.
    /// </summary>
    public class ZipQueueMessage
    {
        /// <summary>
        /// The ID of the note associated with the zip request.
        /// </summary>
        [JsonSchemaType(typeof(string))]
        [JsonProperty("noteId")]
        public Guid NoteId { get; set; } = Guid.Empty;

        /// <summary>
        /// The name of the zip file to be created.
        /// </summary>
        [JsonSchemaType(typeof(string))]
        [JsonProperty("zipFileId")]
        public string ZipFileId { get; set; } = String.Empty;
    }
}
