using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;
using NJsonSchema.Annotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HW4NoteKeeper.Models
{
    /// <summary>
    /// Represents a tag that can be associated with a note. As of 2/25 (JB), tags are generated using AI from the details of the note.
    /// </summary>
    public class Tag
    {
        /// <summary>
        /// Id of the tag.
        /// </summary>
        [Key]
        [Required]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        [JsonSchemaType(typeof(string))]
        [JsonProperty("id")]
        public Guid Id { get; set; }

        /// <summary>
        /// The unique identifier for the note. Tags are one to many with notes.
        /// </summary>
        [ForeignKey("NoteId")]
        [JsonSchemaType(typeof(string))]
        [JsonProperty("noteId")]
        public Guid NoteId { get; set; }

        /// <summary>
        /// The note associated with the tag.  This allows navigation from a tag to the note.  N
        /// </summary>
        public virtual Note Note { get; set; } = null!;

        /// <summary>
        /// The name / value of the tag.
        /// </summary>
        [Required]
        [StringLength(30, MinimumLength = 1)]
        [JsonSchemaType(typeof(string))]
        [JsonProperty("name")]
        public required string Name { get; set; }
    }
}
