using System.Text.Json;
using HW4NoteKeeper.Interfaces;
using HW4NoteKeeper.Models;
using Microsoft.ApplicationInsights;

namespace HW4NoteKeeper.Tools.Validators
{
    /// <summary>
    /// Validates a note. Summary and details must be valid input, see ValidateDetails and ValidateSummary.
    /// </summary>
    public class NoteValidator : INoteValidator
    {
        /// <summary>
        /// Application insights telemetry client.
        /// </summary>
        private readonly TelemetryClient _telemetry;

        public NoteValidator(TelemetryClient telemetry)
        {
            _telemetry = telemetry;
        }

        /// <summary>
        /// Validates a newly created note. Summary and details must be valid input, see ValidateDetails and ValidateSummary.
        /// </summary>
        /// <param name="note">Note to validate able to create</param>
        /// <returns>Returns a list of errors.  If error count is 0, is valid.</returns>
        public List<string> ValidateCreate(DtoNote note)
        {
            var errors = new List<string>();
            errors.AddRange(ValidateDetails(note.Details));
            errors.AddRange(ValidateSummary(note.Summary));

            // Track validation errors
            if (errors.Count > 0)
            {
                TrackValidationErrors("ValidationError on create", errors, note);
            }

            return errors;
        }

        /// <summary>
        /// Validates an update to a note. Summary and /or deails may be NULL, representing they should not be updated.
        /// </summary>
        /// <param name="note">Note to update</param>
        /// <param name="routeDataId">Note ID from route data</param>
        /// <remarks>If summary or details is null, it is assumed intention was not update it and thus is not validated.</remarks>
        /// <returns>Returns a list of errors.  If error count is 0, is valid.</returns>
        public List<string> ValidateUpdate(DtoNote note, Guid routeDataId)
        {
            var errors = new List<string>();

            if (note.Summary != null)
            {
                errors.AddRange(ValidateSummary(note.Summary));
            }

            if (note.Details != null)
            {
                errors.AddRange(ValidateDetails(note.Details));
            }

            // Track validation errors
            if (errors.Count > 0)
            {
               TrackValidationErrors("ValidationError on update", errors, note);
            }

            return errors;
        }

        /// <summary>
        /// Tracks validation errors to application insights.
        /// </summary>
        /// <param name="traceMessage">Message for track trace</param>
        /// <param name="errors">List of errors</param>
        /// <param name="note">Note that caused errors</param>
        private void TrackValidationErrors(string traceMessage, IList<string> errors, DtoNote note)
        {
            var properties = new Dictionary<string, string>
            {
                { "ValidationErrors", string.Join("; ", errors) },
                { "InputPayload", JsonSerializer.Serialize(note) }
            };

            _telemetry.TrackTrace(traceMessage, properties);
        }

        /// <summary>
        /// Validates a summary. Constraints: Required, Not Null, Not Empty, Not All Spaces/Whitespace
        /// </summary>
        /// <param name="summary"></param>
        /// <returns>A list of error messages. If empty, no errors.</returns>
        private List<String> ValidateSummary(string? summary)
        {
            var errors = new List<String>();

            if (summary == null)
            {
                errors.Add("Summary cannot be null.");

                return errors;
            }

            if (summary.Length < 1)
            {
                errors.Add("Summary cannot be empty.");
            }

            if (summary.Length > 60)
            {
                errors.Add("Summary cannot be greater than 60 characters.");
            }

            if (String.IsNullOrWhiteSpace(summary))
            {
                errors.Add("Summary cannot be all white spaces.");
            }

            if (errors.Count > 0)
            {
                errors.Add("Summary constraints: Required, Not Null, Not Empty, Not All Spaces/Whitespace and must be between 1 and 60 characters.");
            }

            return errors;
        }

        /// <summary>
        /// Validates details. Constraints: Required, Not Null, Not Empty, Not All Spaces/Whitespace
        /// </summary>
        /// <param name="details">details for the note</param>
        /// <returns>Returns a list of errors messages. If empty, no errrors</returns>
        private List<string> ValidateDetails(string? details)
        {
            var errors = new List<String>();

            if (details == null)
            {
                errors.Add("details cannot be null.");

                return errors;
            }

            if (details.Length < 1)
            {
                errors.Add("details cannot be empty.");
            }

            if (details.Length > 1024)
            {
                errors.Add("details cannot be greater than 1024 characters.");
            }

            if (String.IsNullOrWhiteSpace(details))
            {
                errors.Add("details cannot be all white spaces.");
            }

            if (errors.Count > 0)
            {
                errors.Add("Details constraints: Required, Not Null, Not Empty, Not All Spaces/Whitespace and must be between 1 and 1024 characters.");
            }

            return errors;
        }
    }
}
