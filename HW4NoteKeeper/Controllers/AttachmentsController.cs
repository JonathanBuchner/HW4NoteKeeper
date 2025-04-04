using System.Net.Mail;
using HW4NoteKeeper.ApplicationInsightsTrackers;
using HW4NoteKeeper.Data;
using HW4NoteKeeper.Enums;
using HW4NoteKeeper.Infrastructure.Services;
using HW4NoteKeeper.Infrastructure.Settings;
using HW4NoteKeeper.Interfaces;
using HW4NoteKeeper.Models;
using HW4NoteKeeper.Validators;
using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Mvc;

namespace HW4NoteKeeper.Controllers
{
    [ApiController]
    [Route("notes/{noteId}/attachments")]
    public class AttachmentsController : BaseController
    {
        private readonly AttachmentValidator _attachmentValidator;

        private readonly IApplicationLogger<DtoAttachment> _al;

        public AttachmentsController(
            MyOpenAiClient aiClient, 
            TelemetryClient telemetryClient, 
            NotesAppDatabaseContext dbContext, 
            NoteSettings efSettings,
            ILogger<AttachmentsController> logger,
            IAzureStorageDataAccessLayer azureStorageDataAccessLayer)
            : base(aiClient, telemetryClient, dbContext, efSettings, azureStorageDataAccessLayer)
        {
            _attachmentValidator = new AttachmentValidator(telemetryClient);
            _al = new ApplicationLogger<DtoAttachment>(telemetryClient, logger);
        }

        [HttpGet("{attachmentId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAttachment(Guid noteId, string attachmentId)
        {
            try
            {
                var errors = _attachmentValidator.ValidateAttachmentIds(noteId, attachmentId);

                if (errors.Count > 0)
                {
                    return NotFoundCustomResponse(_al, "Attachment not found", $" Request for {attachmentId} not found in blob {noteId}.");
                }

                var fileStream = await _azureStorageDataAccessLayer.GetAttachment(noteId, attachmentId);
                
                if (fileStream == null)
                {
                    return NotFoundCustomResponse(_al, "Attachment not found", $"Attachment {attachmentId} not found in blob {noteId}.");
                }

                return Ok(fileStream);
            }
            catch (Exception ex)
            {
                return InternalServerErrorExceptionCustomResponse(_al, new DtoAttachment { NoteId = noteId, AttachmentId = attachmentId }, ex);
            }
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAttachments(Guid noteId)
        {
            try
            {   
                var note = _noteRepository.GetNote(noteId);

                if (note == null)
                {
                    return NotFoundCustomResponse(_al, "Note not found", $"Note {noteId} not found.");
                }

                var attachments = await _azureStorageDataAccessLayer.GetNoteAttachmentDetails(noteId);

                if (attachments == null)
                {
                    return NotFoundCustomResponse(_al, "Attachments not found", $"Attachments not found in blob {noteId}.");
                }
                return Ok(attachments);
            }
            catch (Exception ex)
            {
                return InternalServerErrorExceptionCustomResponse(_al, new DtoAttachment { NoteId = noteId }, ex);
            }
        }


        /// <summary>
        /// Upload or create attachment to note.
        /// </summary>
        /// <param name="noteId">Id of the note. This will be the container name.</param>
        /// <param name="attachmentId">This is the file name and will be the name of the blob.</param>
        /// <param name="file">IFormFile file to upload to the blob.</param>
        /// <returns></returns>
        [HttpPut("{attachmentId}")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> PutAttachment(Guid noteId, string attachmentId, IFormFile file)
        {
            var dtoAttachment = new DtoAttachment
            {
                NoteId = noteId,
                AttachmentId = attachmentId,
                FileData = file
            };

            try
            { 
                // Validate
                var errors = _attachmentValidator.ValidateDto(dtoAttachment);

                // If errors exist, return bad request
                if (errors.Count > 0)
                {
                   return InvalidAttachmentBadRequestResponse(new DtoAttachment { NoteId = noteId, AttachmentId = attachmentId}, errors);   // Do not log the file data
                }

                // Check note exists (null is return if note does not exist). If note does not exists, return not found response.
                var note = _noteRepository.GetNote(noteId);
                if (note == null)
                {
                    return CannotFindNoteNotFoundResponse(new DtoAttachment { NoteId = noteId, AttachmentId = attachmentId });              // Do not log the file data
                }

                // Save attachment
                var response = await _azureStorageDataAccessLayer.PutAttachment(dtoAttachment);

                // Return response based on data access layer.
                return HandleBlobStorageUploadResponse(response, dtoAttachment);

            }
            catch (Exception ex)
            {
                return InternalServerErrorExceptionCustomResponse(_al, new DtoAttachment { NoteId = noteId, AttachmentId = attachmentId }, ex);      // Do not log the file data
            }
        }

        /// <summary>
        /// Delete attachment from note.
        /// </summary>
        /// <param name="noteId">Id of the note. This will be the container name.</param>
        /// <param name="attachmentId">This was the file name and is the name of the blob.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        [HttpDelete("{attachmentId}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteAttachment(Guid noteId, string attachmentId)
        {
            try
            {
                // Check note exists.
                var note = _noteRepository.GetNote(noteId);
                if (note == null)
                {
                    return CannotFindNoteNotFoundResponse(new DtoAttachment { NoteId = noteId, AttachmentId = attachmentId });
                }

                // Delete attachment
                var response = await _azureStorageDataAccessLayer.DeleteAttachment(noteId, attachmentId);

                return HandleblobStorageDeleteResponse(response, noteId, attachmentId);
            }
            catch (Exception ex)
            {
                return InternalServerErrorExceptionCustomResponse(_al, new DtoAttachment { NoteId = noteId, AttachmentId = attachmentId }, ex);
            }
        }

        /// <summary>
        /// Invalid attachment object bad request response for creating/updating an attachment.
        /// </summary>
        /// <param name="dtoAttachment">attachment</param>
        /// <param name="errors">list of errors</param>
        /// <returns></returns>
        private IActionResult InvalidAttachmentBadRequestResponse(DtoAttachment dtoAttachment, List<string> errors)
        {
            if (errors.Count == 0)
            {
                throw new ArgumentException("Errors must be provided for invalid attachment bad request response.");
            }

            Dictionary<string, object> errorDictionary = new() { { "error", errors } };
            var title = "Invalid attachment object";
            
            return BadRequestCustomResponse(_al, title, title, dtoAttachment, errorDictionary);
        }

        /// <summary>
        /// Note not found response for creating/updating an attachment. Sent if a note is not found for the corresponding attachment.
        /// </summary>
        /// <param name="dtoAttachment">payload</param>
        /// <returns></returns>
        private IActionResult CannotFindNoteNotFoundResponse(DtoAttachment dtoAttachment)
        {
            var title = "Note not found";
            var message = $"Note with id {dtoAttachment.NoteId} not found. Cannot complete request.";
            
            return NotFoundCustomResponse(_al, title, message, dtoAttachment);
        }


        /// <summary>
        /// Handle the response from the blob storage upload. Uses BlobStorageResponse enum to determine the response.
        /// </summary>
        /// <param name="response">Blob storage response</param>
        /// <param name="dtoAttachment">payload</param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException">Throws argument out of range exception</exception>
        private IActionResult HandleBlobStorageUploadResponse(BlobStorageResponseUpdateCreate response, DtoAttachment dtoAttachment)
        {
            switch (response)
            {
                case BlobStorageResponseUpdateCreate.Created:
                    Response.Headers.Location = $"{Request.Scheme}://{Request.Host}/notes/{dtoAttachment.NoteId}/attachments/{dtoAttachment.AttachmentId}";
                    return StatusCode(StatusCodes.Status201Created);

                case BlobStorageResponseUpdateCreate.Updated:
                    return NoContent();

                case BlobStorageResponseUpdateCreate.RejectedMaxSizeExceeded:
                    var title = "Attachment limit reached";
                    var message = $"Attachment limit reached MaxAttachments: {_azureStorageDataAccessLayer.GetMaxAttachments()}";

                    return ForbiddenCustomResponse(_al, title, message);

                default:
                    throw new ArgumentOutOfRangeException("Unknown response for Attachment valid");
            }
        }

        private IActionResult HandleblobStorageDeleteResponse(BlobStorageResponseDelete response, Guid noteId, string attachmentId)
        {
            switch (response)
            {
                case BlobStorageResponseDelete.Deleted:
                    return NoContentLogCustomResponse(_al, "Successful attachment delete", $"Successfully delete attachment {attachmentId} from blob {noteId}.");

                case BlobStorageResponseDelete.NotFound:
                    return NoContentWarnCustomResponse(_al, "Failed attachment delete", $"Attachment {attachmentId} not found in blob {noteId}.");

                default:
                    throw new ArgumentOutOfRangeException("Unknown response for Attachment delete");
            }
        }
    }
}
