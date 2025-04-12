using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;
using NJsonSchema.Annotations;

namespace HW4NoteKeeperEx2.Models
{
    /// <summary>
    /// A note that can be created, updated, and deleted.  Tags are generated from details and the tagging ai service as of 2/13.
    /// </summary>
    public class Note
    {
        /// <summary>
        /// The unique identifier for the note.
        /// </summary>
        [Key]
        [Required]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        [JsonSchemaType(typeof(string))]
        [JsonProperty("noteId")]
        public Guid NoteId { get; set; }

        /// <summary>
        /// A brief summary of the note.
        /// </summary>
        [Required]
        [StringLength(60, MinimumLength = 1)]
        [JsonProperty("summary")]
        public string Summary { get; set; } = "";

        /// <summary>
        /// The details of the note. Details are used to generate tags.
        /// </summary>
        [Required]
        [StringLength(1024, MinimumLength = 1)]
        [JsonProperty("details")]
        public string Details { get; set; } = "";

        /// <summary>
        /// The date and time the note was created.
        /// </summary>
        [JsonSchemaType(typeof(string))]
        [JsonProperty("createdDateUtc")]
        public DateTimeOffset? CreatedDateUtc { get; set; }

        /// <summary>
        /// The date and time the note was last modified. May be updated even if the note is not changed.
        /// </summary>
        [JsonSchemaType(typeof(string))]
        [JsonProperty("modifiedDateUtc")]
        public DateTimeOffset? ModifiedDateUtc { get; set; }

        /// <summary>
        /// The tags associated with the note.  Only should be updated if the details are updated. Generated from details.
        /// </summary>
        [JsonProperty("tags")]
        public ICollection<Tag>? Tags { get; set; } = new List<Tag>();
    }
}
