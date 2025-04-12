using HW4NoteKeeperEx2.Models;

namespace HW4NoteKeeperEx2.Interfaces
{
    /// <summary>
    /// Interface for validating notes.
    /// </summary>
    public interface INoteValidator
    {
        /// <summary>
        /// Validates a newly created note. Summary and details must be valid input, see ValidateDetails and ValidateSummary.
        /// </summary>
        /// <param name="note">Note to validate able to create</param>
        /// <returns>Returns a list of errors.  If error count is 0, is valid.</returns>
        List<string> ValidateCreate(DtoNote note);

        /// <summary>
        /// Validates an update to a note. Summary and /or deails may be NULL, representing they should not be updated.
        /// </summary>
        /// <param name="note">Note to update</param>
        /// <param name="routeDataId">Note ID from route data</param>
        /// <remarks>If summary or details is null, it is assumed intention was not update it and thus is not validated.</remarks>
        /// <returns>Returns a list of errors.  If error count is 0, is valid.</returns>
        List<string> ValidateUpdate(DtoNote note, Guid routeDataId);
    }
}
