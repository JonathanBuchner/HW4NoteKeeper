namespace HW4NoteKeeper.Models
{
    /// <summary>
    /// Search parameters for notes. As of 2/25 (JB), only tag name is used for refining search.
    /// </summary>
    public class NoteSearchParameters
    {
        /// <summary>
        /// The tag name for refining searches.
        /// </summary>
        public string? TagName { get; set; }
    }
}
