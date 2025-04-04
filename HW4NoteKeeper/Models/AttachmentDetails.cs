using Newtonsoft.Json;
using NJsonSchema.Annotations;

namespace HW4NoteKeeper.Models
{
    /// <summary>
    /// Attachment details
    /// </summary>
    public class AttachmentDetails
    {
        /// <summary>
        /// Attachment ID
        /// </summary>
        [JsonSchemaType(typeof(string))]
        [JsonProperty("attachmentId")]
        public string AttachmentId { get; set; } = string.Empty;

        [JsonSchemaType(typeof(string))]
        [JsonProperty("contentType")]
        public string ContentType { get; set; } = string.Empty;

        [JsonSchemaType(typeof(string))]
        [JsonProperty("createdDate")]
        public DateTimeOffset CreatedDate { get; set; }

        [JsonSchemaType(typeof(string))]
        [JsonProperty("lastModifiedDate")]
        public DateTimeOffset LastModifiedDate { get; set; }

        [JsonSchemaType(typeof(string))]
        [JsonProperty("length")]
        public string Length { get; set; }
    }
}
