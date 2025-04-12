using Newtonsoft.Json;
using NJsonSchema.Annotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography.Xml;

namespace HW4NoteKeeperEx2.Models
{
    /// <summary>
    /// Data transfer object for a note. As of 2/25 (JB), user only sees tag names.  Key difference compared to Note is list strings rather than list of  tags.
    /// </summary>
    public class DtoNote
    {
        /// <summary>
        /// The unique identifier for the note.
        /// </summary>
        [JsonSchemaType(typeof(string))]
        [JsonProperty("noteId")]
        public Guid? NoteId { get; set; }

        /// <summary>
        /// A brief summary of the note.
        /// </summary>
        [StringLength(60, MinimumLength = 1)]
        [JsonProperty("summary")]
        public string? Summary { get; set; }

        /// <summary>
        /// The details of the note. Details are used to generate tags.
        /// </summary>
        [StringLength(1024, MinimumLength = 1)]
        [JsonProperty("details")]
        public string? Details { get; set; }

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
        /// The tags associated with the note.  Only should be updated if the details are updated. Generated from details. Dto object should be collection of strings
        /// </summary>
        [JsonProperty("tags")]
        public ICollection<string>? Tags { get; set; } = new List<string>();

        public DtoNote() { }

        public DtoNote(Note note)
        {
            if (note == null)
            {
                throw new ArgumentNullException(nameof(note));
            }

            NoteId = note.NoteId;
            Summary = note.Summary;
            Details = note.Details;
            CreatedDateUtc = note.CreatedDateUtc;
            ModifiedDateUtc = note.ModifiedDateUtc;

            // Use .AsNoTracking() in the repository layer when retrieving Note objects to avoid unintended database hits
            // JB 2/25 Because we are using ling, we don't want to accidentally ask the database for the tags again.
            Tags = note.Tags?.Select(tag => tag.Name).ToList() ?? new List<string>();           

        }
    }
}
