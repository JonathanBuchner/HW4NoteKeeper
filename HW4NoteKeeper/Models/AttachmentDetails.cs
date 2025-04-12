using Newtonsoft.Json;
using NJsonSchema.Annotations;

namespace HW4NoteKeeperEx2.Models
{
    /// <summary>
    /// Attachment details
    /// </summary>
    public class AttachmentDetails
    {
        /// <summary>
        /// Attachment ID.
        /// </summary>
        [JsonSchemaType(typeof(string))]
        [JsonProperty("attachmentId")]
        public string AttachmentId { get; set; } = string.Empty;

        /// <summary>
        /// Content type of the attachment.
        /// </summary>
        [JsonSchemaType(typeof(string))]
        [JsonProperty("contentType")]
        public string ContentType { get; set; } = string.Empty;


        /// <summary>
        /// Created date of the attachment.
        /// </summary>
        [JsonSchemaType(typeof(string))]
        [JsonProperty("createdDate")]
        public DateTimeOffset CreatedDate { get; set; }

        /// <summary>
        /// Last modified date of the attachment.
        /// </summary>
        [JsonSchemaType(typeof(string))]
        [JsonProperty("lastModifiedDate")]
        public DateTimeOffset LastModifiedDate { get; set; }

        /// <summary>
        /// Size of the attachment.
        /// </summary>
        [JsonSchemaType(typeof(string))]
        [JsonProperty("length")]
        public string Length { get; set; } = string.Empty;
    }
}
