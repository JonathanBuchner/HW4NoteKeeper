using System.ComponentModel.DataAnnotations;
using System.Diagnostics.Eventing.Reader;
using System.Reflection.Metadata.Ecma335;
using System.Text.Json;
using System.Text.RegularExpressions;
using Azure.Storage.Blobs;
using HW4NoteKeeperEx2.Interfaces;
using HW4NoteKeeperEx2.Models;
using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Mvc;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace HW4NoteKeeperEx2.Validators
{
    /// <summary>
    /// Validator for attachments.
    /// </summary>
    public class AttachmentValidator : IAttachmentValidator
    {
        private readonly TelemetryClient _telemetry;
        private const int MaxBlobNameLength = 1024; 
        private const int MaxPathSegmentsWithHNS = 63; 

        // Static methods

        /// <summary>
        /// Check if attachment is valid.
        /// </summary>
        /// <param name="attachment">attachment to validate</param>
        /// <returns>returns true if attachment is valid</returns>
        public static Attachment GetAttachmentFromDto(DtoAttachment attachment)
        {
            var errors = ValidateDtoAttachment(attachment);

            if (errors.Count > 0)
            {
                throw new ValidationException(string.Join("; ", errors));
            }

            return new Attachment
            {
                NoteId = attachment?.NoteId ?? Guid.Empty,
                AttachmentId = attachment?.AttachmentId ?? "",
                FileData = attachment?.FileData ?? new FormFile(Stream.Null, 0, 0, "", "")
            };
        }

        /// <summary>
        /// Validate the ids of the attachment.
        /// </summary>
        /// <param name="noteId">note id</param>
        /// <param name="attachmentId">attachment id</param>
        /// <returns>List of errors</returns>
        private static List<string> ValidateDtoIds(Guid? noteId, string attachmentId)
        {
            List<string> errors = new List<string>();

            // NoteId validation

            if (noteId == null || noteId == Guid.Empty)
            {
                errors.Add("NoteId is required.");
            }

            // Blob name validation
            if (string.IsNullOrWhiteSpace(attachmentId))
            {
                errors.Add("Blob name cannot be null, empty, or whitespace.");
            }
            else
            {
                var name = attachmentId;

                // length constraints
                if (name.Length < 1 || name.Length > MaxBlobNameLength)
                {
                    errors.Add($"Blob name must be between 1 and {MaxBlobNameLength} characters long.");
                }

                // URL encoding (reserved characters must be escaped)
                if (!IsValidUrlEncoding(name))
                {
                    errors.Add("Blob name contains invalid URL characters. Reserved URL characters must be properly escaped.");
                }

                //Path segment

                // Path segment limits
                var pathSegments = name.Split('/');
                if (pathSegments.Length > MaxPathSegmentsWithHNS)
                {
                    errors.Add($"Blob name has too many path segments. Maximum allowed: {MaxPathSegmentsWithHNS}.");
                }

                // No path segment ends with a dot (.) or forward slash (/)
                if (pathSegments.Any(segment => segment.EndsWith('.') || segment.EndsWith('/')))
                {
                    errors.Add("No path segments should end with a dot (.) or a forward slash (/).");
                }

                // Ensure blob name does not end with a dot (.) or forward slash (/)
                if (name.EndsWith('.') || name.EndsWith('/'))
                {
                    errors.Add("Blob name cannot end with a dot (.) or forward slash (/).");
                }
            }

            return errors;
        }
        /// <summary>
        /// Validate attachment.
        /// </summary>
        /// <param name="attachment">attachment to validate</param>
        /// <remarks>blob nameing conventions: https://learn.microsoft.com/en-us/rest/api/storageservices/naming-and-referencing-containers--blobs--and-metadata#blob-names</remarks>
        /// <returns>returns list of errors when validating</returns>
        private static List<string> ValidateDtoAttachment(DtoAttachment attachment)
        {
            var errors = new List<string>();

            if (attachment == null)
            {
                errors.Add("Attachment is null. Attachment cannot be null.");

                return errors;
            }

            errors.AddRange(ValidateDtoIds(attachment.NoteId, attachment.AttachmentId));

            if (attachment.FileData == null)
            {
                errors.Add("FileData is required.");
            }

            return errors;
        }

        // Public methods

        public AttachmentValidator(TelemetryClient telemetry)
        {
            _telemetry = telemetry;
        }

        /// <summary>
        ///  Validate attachment for create.  Will track validation errors. 
        /// </summary>
        /// <param name="attachment">attachment to validate</param>
        /// <returns>list of errors</returns>
        public List<string> ValidateDto(DtoAttachment attachment)
        {
            var errors = ValidateDtoAttachment(attachment);

            if (errors.Count > 0)
            {
                TrackValidationErrors("ValidationErrors for putting a dtoAttachment", errors, attachment);
            }

            return errors;
        }

        public List<string> ValidateAttachmentIds(Guid noteId, string attachmentId)
        {
            return ValidateDtoIds(noteId, attachmentId);
        }

        /// <summary>
        /// Logs validation errors.
        /// </summary>
        /// <param name="message">The message to track</param>
        /// <param name="errors">The list of errors</param>
        /// <param name="attachment">The object with the error</param>
        private void TrackValidationErrors(string message, List<string> errors, DtoAttachment attachment)
        {
            var properties = new Dictionary<string, string>
                {
                    { "NoteId", attachment.NoteId?.ToString() ?? ""  },
                    { "AttachmentId", attachment.AttachmentId?.ToString() ?? ""},
                    { "FileData", attachment.FileData?.ToString() ?? "(No file data)" }
                };

            _telemetry.TrackEvent(message, properties);
            _telemetry.TrackTrace(JsonSerializer.Serialize(errors));
        }

        private static bool IsValidUrlEncoding(string blobName)
        {
            string pattern = @"^[a-zA-Z0-9\-._~!$&'()*+,;=:@/]*$";
            return Regex.IsMatch(blobName, pattern);
        }
    }
}
