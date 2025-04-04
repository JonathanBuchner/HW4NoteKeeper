using Newtonsoft.Json;
using NJsonSchema.Annotations;
using System.ComponentModel.DataAnnotations;

namespace HW4NoteKeeper.Models
{
    /// <summary>
    /// Data transfer object for a tag. As of 2/25 (JB), user only sees the name of the tag as compared to the full tag object.
    /// </summary>
    public class DtoTag
    {
        /// <summary>
        /// The name / value of the tag.
        /// </summary>
        [Required]
        [StringLength(30, MinimumLength = 1)]
        [JsonSchemaType(typeof(string))]
        [JsonProperty("name")]
        public string Name { get; set; }

        public DtoTag(Tag tag)
        {
            Name = tag.Name;
        }
    }
}
