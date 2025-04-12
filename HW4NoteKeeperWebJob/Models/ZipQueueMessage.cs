using Newtonsoft.Json;
using NJsonSchema.Annotations;

namespace HW4NoteKeeperWebJob.Models
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

        [JsonSchemaType(typeof(string))]
        [JsonProperty("zipFileId")]
        /// <summary>
        /// The name of the zip file to be created.
        /// </summary>
        public string ZipFileId { get; set; } = String.Empty;
    }
}
