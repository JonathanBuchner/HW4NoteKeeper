using HW4NoteKeeperEx2.MetricTrackers;
using HW4NoteKeeperEx2.Models;

namespace HW4NoteKeeperEx2.Interfaces
{
    /// <summary>
    /// Interface for the Note Data Access Layer.
    /// </summary>
    public interface INoteDataAccessLayer
    {
        /// <summary>
        /// Creates a note in the data store.
        /// </summary>
        /// <param name="DtoNote">DtoNote to create</param>
        /// <returns>Returns the new DtoNote</returns>
        public Task<DtoNote> CreateNote(DtoNote DtoNote);
        
        /// <summary>
        /// Updates a note in the data store.  
        /// </summary>
        /// <remarks>Will track changes to the note to application insights.</remarks>
        /// <param name="note">DtoNote to update</param>
        /// <returns>Returns true update was made</returns>
        public Task UpdateNote(DtoNote note);
        
        /// <summary>
        /// Deletes a note from the data store.
        /// </summary>
        /// <remarks>Will track updates to the note to application insights.</remarks>
        /// <param name="noteId">Note to update</param>
        /// <returns>Returns true if successful.</returns>
        public Task DeleteNote(Guid noteId);
        
        /// <summary>
        /// Gets a note from the data store.
        /// </summary>
        /// <param name="noteId">note id to used to retrieve note.</param>
        /// <returns>Returns Note if found. If null, no note was found.</returns>
        public Task<DtoNote?> GetNote(Guid noteId);
        
        /// <summary>
        /// Gets all notes from the data store.
        /// </summary>
        /// <param name="parameters">Parameters to search notes.</param>
        /// <returns>Returns all notes that meet search criteria</returns>
        public Task<List<DtoNote>> GetNotes(NoteSearchParameters parameters);

        /// <summary>
        /// Gets all tags from the data store.
        /// </summary>
        /// <returns>Returns List string with tag names.</returns>
        public Task<List<DtoTag>> GetUniqueTags();
    }
}
