using Newtonsoft.Json;
using NJsonSchema.Annotations;

namespace HW4NoteKeeperEx2.Models
{
    /// <summary>
    /// Class representing the details of a zip attachment attached to a note
    /// </summary>
    public class DtoZipAttachmentDetails
    {
        /// <summary>
        /// The unique identifier for the zip file.
        /// </summary>
        [JsonSchemaType(typeof(string))]
        [JsonProperty("zipFileId")]
        public string ZipFileId { get; set; } = string.Empty;

        /// <summary>
        /// The content type of the zip file.
        /// </summary>
        [JsonSchemaType(typeof(string))]
        [JsonProperty("contentType")]
        public string ContentType { get; set; } = string.Empty;

        /// <summary>
        /// Created date of the zip file.
        /// </summary>
        [JsonSchemaType(typeof(string))]
        [JsonProperty("createdDate")]
        public DateTimeOffset CreatedDate { get; set; }

        /// <summary>
        /// The last modified date of the zip file.
        /// </summary>
        [JsonSchemaType(typeof(string))]
        [JsonProperty("lastModifiedDate")]
        public DateTimeOffset LastModifiedDate { get; set; }

        /// <summary>
        /// The length of the zip file in blob  storage as recorded by the blob SDK
        /// </summary>
        [JsonSchemaType(typeof(string))]
        [JsonProperty("Length")]
        public long Length { get; set; }
    }
}
