using HW4NoteKeeper.ApplicationInsightsTrackers;
using HW4NoteKeeper.Data;
using HW4NoteKeeper.Enums;
using HW4NoteKeeper.Infrastructure.Services;
using HW4NoteKeeper.Infrastructure.Settings;
using HW4NoteKeeper.Interfaces;
using HW4NoteKeeper.Models;
using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Mvc;
using System.Net.Mail;

namespace HW4NoteKeeper.Controllers
{
    [ApiController]
    [Route("notes/{noteId}/attachmentzipfiles")]
    public class AttachmentZipController : BaseController
    {
        private readonly IZipRequestQueueService _zipRequest;

        private readonly IApplicationLogger<ZipQueueMessage> _al;

        public AttachmentZipController(
            MyOpenAiClient aiClient,
            TelemetryClient telemetryClient,
            NotesAppDatabaseContext dbContext,
            NoteSettings efSettings,
            ILogger<AttachmentsController> logger,
            IAzureStorageDataAccessLayer azureStorageDataAccessLayer,
            IZipRequestQueueService zipRequest)
            : base(aiClient, telemetryClient, dbContext, efSettings, azureStorageDataAccessLayer)
        {
            _zipRequest = zipRequest;
            _al = new ApplicationLogger<ZipQueueMessage>(telemetryClient, logger);
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CreateZip(Guid noteId)
        {
            var zipFileName = $"{Guid.NewGuid()}.zip";

            try
            {
                // Check if the note exists
                var note = await _noteRepository.GetNote(noteId);

                if (note == null)
                {
                    return CannotFindNoteNotFoundResponse(noteId);
                }

                var attachmentDetails = await _azureStorageDataAccessLayer.GetNoteAttachmentDetails(noteId);

                if (attachmentDetails == null || attachmentDetails.Count() == 0)
                {
                    return NoContent();
                }

                await _zipRequest.EnqueueAsync(noteId, zipFileName);


                Response.Headers.Location = $"{Request.Scheme}://{Request.Host}/notes/{noteId}/attachmentzipfiles/{zipFileName}";
                return Accepted();
            }
            catch (Exception ex)
            { 
                return InternalServerErrorExceptionCustomResponse(_al, new ZipQueueMessage() { NoteId = noteId, ZipFileId = zipFileName }, ex);
            }

        }

        [HttpDelete("{zipFileName}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteZip(Guid noteId, string zipFileName)
        {
            try
            {
                // Added requirement.  Check if zip file name is valid.
                if (string.IsNullOrEmpty(zipFileName))
                {
                    return BadRequestCustomResponse(_al, "Invalid zip file name", "Zip file name cannot be null or empty.", new ZipQueueMessage() { NoteId = noteId, ZipFileId = zipFileName });
                }

                // Check if attachemt exists
                if (!await _azureStorageDataAccessLayer.CheckIfBlobExists(noteId, zipFileName))
                {
                    return CannotFindAttachementResponse(noteId, zipFileName);
                }

                var response = await _azureStorageDataAccessLayer.DeleteAttachment(noteId, zipFileName);

                return HandleblobStorageDeleteResponse(response, noteId, zipFileName);
            }
            catch (Exception ex)
            {
                return InternalServerErrorExceptionCustomResponse(_al, new ZipQueueMessage() { NoteId = noteId, ZipFileId = zipFileName }, ex);
            }
        }

        /// <summary>
        /// Returns a 404 Not Found response when the note is not found. Will log warning
        /// </summary>
        /// <param name="noteId">note id</param>
        /// <returns>not found response</returns>
        private IActionResult CannotFindNoteNotFoundResponse(Guid noteId)
        {
            var title = "Note not found";
            var message = $"Note with id {noteId} not found. Cannot complete request.";

            return NotFoundCustomResponse(_al, title, message, new ZipQueueMessage() { NoteId = noteId });
        }

        /// <summary>
        /// Handle the response from the blob storage delete. Uses BlobStorageResponse enum to determine the response.
        /// </summary>
        /// <param name="response">Blob storage response</param>
        /// <param name="noteId">note id</param>
        /// <param name="zipFileName">attachment id</param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        private IActionResult HandleblobStorageDeleteResponse(BlobStorageResponseDelete response, Guid noteId, string zipFileName)
        {
            switch (response)
            {
                case BlobStorageResponseDelete.Deleted:
                    return SuccesfullyDeletedAttachmentNoContentResponse(noteId, zipFileName);
                    

                case BlobStorageResponseDelete.NotFound:
                    return CannotFindAttachementResponse(noteId, zipFileName);

                default:
                    throw new ArgumentOutOfRangeException("Unknown response for Attachment delete");
            }
        }

        private IActionResult CannotFindAttachementResponse(Guid noteId, string attachmentId)
        {
            var title = "Attachment not found";
            var message = $"Attachment with id {attachmentId} not found. Cannot complete request.";

            return NotFoundCustomResponse(_al, title, message, new ZipQueueMessage() { NoteId = noteId });
        }

        /// <summary>
        /// Returns a 204 No Content response when the attachment is deleted successfully. Will log information
        /// </summary>
        /// <param name="noteId">note id</param>
        /// <param name="zipFileName">zip file file name</param>
        /// <returns>No content response</returns>
        private IActionResult SuccesfullyDeletedAttachmentNoContentResponse(Guid noteId, string zipFileName)
        {
            var title = "Attachment deleted";
            var message = $"Attachment with id {zipFileName} deleted from note {noteId}.";

            return NoContentLogCustomResponse(_al, title, message, new ZipQueueMessage() { NoteId = noteId, ZipFileId = zipFileName });
        }
            
    }
}
