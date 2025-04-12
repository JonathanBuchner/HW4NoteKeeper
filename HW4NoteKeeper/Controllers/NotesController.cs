using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using HW4NoteKeeper.Models;
using HW4NoteKeeper.Interfaces;
using HW4NoteKeeper.Tools.Validators;
using Microsoft.ApplicationInsights;
using HW4NoteKeeper.ApplicationInsightsTrackers;
using HW4NoteKeeper.Data;
using HW4NoteKeeper.CustomExceptions;
using HW4NoteKeeper.Infrastructure.Settings;
using HW4NoteKeeper.Infrastructure.Services;
using HW4NoteKeeper.DataAccessLayer;
using System.Diagnostics;

namespace HW4NoteKeeper.Controllers
{
    /// <summary>
    /// Controller for handling notes.  Follows rest conventions.
    /// </summary>
    [ApiController]
    [Route("[controller]")]
    public class NotesController : BaseController
    {
        /// <summary>
        /// Note validator for validating notes.
        /// </summary>
        private readonly INoteValidator _noteValidator;

        /// <summary>
        /// Exception tracker for tracking exceptions for notes.
        /// </summary>
        private readonly IApplicationLogger<DtoNote> _aiNoteExceptionTracker;

        public NotesController(
            MyOpenAiClient aiClient,
            TelemetryClient telemetryClient, 
            NotesAppDatabaseContext dbContext, 
            NoteSettings efSettings, 
            ILogger<NotesController> logger,
            IAzureStorageDataAccessLayer azureStorageDataAccessLayer)
            : base(aiClient, telemetryClient, dbContext, efSettings, azureStorageDataAccessLayer)
        {
            _noteValidator = new NoteValidator(telemetryClient);
            _aiNoteExceptionTracker = new ApplicationLogger<DtoNote>(telemetryClient, logger);
        }

        // GET: NotesController/{id}
        /// <summary>
        /// Retrieves notes by identifier; otherwise, returns returns 404 not found.  Will return 500 internal server error with ProblemDetails if exception occurs.
        /// </summary>
        /// <param name="id">Note identifier.</param>
        /// <returns>Returns Note if found.</returns>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(DtoNote), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(void), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<DtoNote>> Get(Guid id)
        {
            try
            { 
                var note = await _noteRepository.GetNote(id);

                if (note == null)
                {
                    return NotFound();
                }

                return Ok(note);
            }
            catch (Exception ex)
            {
                return InternalServerErrorExceptionCustomResponse(_aiNoteExceptionTracker, new DtoNote() { NoteId = id }, ex);
            }
        }

        // GET: NotesController/
        /// <summary>
        /// Retrieves all notes in the data store. Optionally filter by tagName. Will return 500 internal server error with ProblemDetails if exception occurs.
        /// </summary>
        /// <returns>Returns all notes.</returns>
        [HttpGet]
        [ProducesResponseType(typeof(List<DtoNote>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(void), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<List<DtoNote>>> GetAll([FromQuery] string? tagName = null)
        {
            try
            {
                var parameters = new NoteSearchParameters() { 
                    TagName =  tagName
                };

                var notes = await _noteRepository.GetNotes(parameters);

                return Ok(notes);
            }
            catch (Exception ex)
            {
                return InternalServerErrorExceptionCustomResponse(_aiNoteExceptionTracker, ex);
            }
        }

        // POST: NotesController/
        /// <summary>
        /// Creates and returns note with a 201 created response. 
        /// Returns 4000 bad request if note payload is improperly formatted; returns list of erros with formatting. 
        /// Returns 403 response with ProblemDetails if note limit has been reached. 
        /// Returns 500 internal with ProblemDetails if there is an error on the server.
        /// </summary>
        /// <param name="newNote">Note to add to datastore.</param>
        /// <returns>Returns newly created note.</returns>
        [HttpPost]
        [SwaggerOperation(Summary = "Creates a note", Description = "Adds a note to the data store.")]
        [ProducesResponseType(typeof(DtoNote), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(string), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(void), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<DtoNote>> Create([FromBody] DtoNote newNote)
        {
            try
            {
                var errors = _noteValidator.ValidateCreate(newNote);

                if (errors.Count > 0)
                {
                    return BadRequest(errors);
                }

                var addedNote = await _noteRepository.CreateNote(newNote);

                return CreatedAtAction(nameof(GetAll), new { id = addedNote.NoteId }, addedNote);
            }
            catch (LimitReachedCustomException ex)
            {
                return ForbiddenExceptionCustomResponse(_aiNoteExceptionTracker, newNote, ex, "Note Limit reached");
            }
            catch (Exception ex)
            {
                return InternalServerErrorExceptionCustomResponse(_aiNoteExceptionTracker, newNote, ex);
            }
        }

        // PATCH: NotesController/{id}
        /// <summary>
        /// Update note in the data store and returns 204 NoContent response. 
        /// If route does not include id, returns MethodNotAllowed. 
        /// Returns bad request update note that does not meet constraints that includes a list of errors. 
        /// Returns 404 respons with Problem details if note cannot be found.
        /// Returns 500 internal server error with ProblemDetails if exception occurs.
        /// </summary>
        /// <param name="id">Note identifier.</param>
        /// <param name="updatedNote">Note to update in data store.</param>
        /// <returns>Returns no content response if succesful.</returns>
        [HttpPatch("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(void), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(string), StatusCodes.Status405MethodNotAllowed)]   // Handled in middeleware
        [ProducesResponseType(typeof(void), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> Update(Guid id, [FromBody] DtoNote updatedNote)
        {
            // Ensure note id is set to the id in the route.
            updatedNote.NoteId = id;
            
            try
            {
                var errors = _noteValidator.ValidateUpdate(updatedNote, id);

                if (errors.Count > 0)
                {
                    return BadRequest(errors);
                }

                await _noteRepository.UpdateNote(updatedNote);

                return NoContent();
            }
            catch (NotFoundCustomException ex)
            {
                return NotFoundExceptionCustomResponse(_aiNoteExceptionTracker, updatedNote, ex, "Note not found");
            }
            catch (Exception ex)
            {
                return InternalServerErrorExceptionCustomResponse(_aiNoteExceptionTracker, updatedNote, ex);
            }
        }

        // DELETE: NotesController/{id}
        /// <summary>
        /// Deletes note from datastore and returns NoContent.
        /// If route does not include id, returns MethodNotAllowed.
        /// If note cannot be found, returns 404 response with ProblemDetails.
        /// If exception occurs, returns 500 internal server error with ProblemDetails.
        /// </summary>
        /// <param name="id">Identifier of note to delete.</param>
        /// <returns>Returns no content response if successful.</returns>
        [HttpDelete("{id}")]
        [SwaggerOperation(Summary = "Deletes a note", Description = "Deletes a note from the data store.")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(string), StatusCodes.Status405MethodNotAllowed)]   // Handled in middeleware
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> Delete(Guid id)
        {
            try
            {
                // Delete note
                await _noteRepository.DeleteNote(id);

                // Delet blobs
                try
                {
                    // Delete the attachment folder
                    await _azureStorageDataAccessLayer.DeleteAttachmentContainer(id);

                    // Deleet the zip folder
                    var isWithZipName = $"{id}-zip";
                    await _azureStorageDataAccessLayer.DeleteAttachmentContainer(isWithZipName);
                }
                catch(Exception ex)
                {
                    //Instructions state: If there is a failure to delete an existing attachment or a failure to delete the container, log an with the details of the failure(s), but do not report a failure to the caller.
                    _aiNoteExceptionTracker.LogException(ex, new DtoNote() { NoteId = id });
                }

                return NoContent();
            }
            catch (NotFoundCustomException ex)
            {
                return NotFoundExceptionCustomResponse(_aiNoteExceptionTracker, new DtoNote() { NoteId = id }, ex, "Note not found");
            }
            catch (Exception ex)
            {
                return InternalServerErrorExceptionCustomResponse(_aiNoteExceptionTracker, new DtoNote() { NoteId = id }, ex);
            }
        }
    }
}
