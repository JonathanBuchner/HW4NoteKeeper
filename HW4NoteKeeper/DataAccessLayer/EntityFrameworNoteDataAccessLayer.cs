using System.Diagnostics;
using HW4NoteKeeper.CustomExceptions;
using HW4NoteKeeper.Data;
using HW4NoteKeeper.Infrastructure.Services;
using HW4NoteKeeper.Infrastructure.Settings;
using HW4NoteKeeper.Interfaces;
using HW4NoteKeeper.MetricTrackers;
using HW4NoteKeeper.Models;
using Microsoft.ApplicationInsights;
using Microsoft.EntityFrameworkCore;

namespace HW4NoteKeeper.Dal
{
    /// <summary>
    /// Data access layer for notes using Entity Framework.
    /// </summary>
    public class EntityFrameworNoteDataAccessLayer : INoteDataAccessLayer
    {
        /// <summary>
        /// OpenAI client for requesting tags.
        /// </summary>
        private readonly MyOpenAiClient _aiClient;

        /// <summary>
        /// Metric tracker for notes.
        /// </summary>
        private readonly INotesMetricTracker _metricTracker;

        /// <summary>
        /// Database context for notes.
        /// </summary>
        private readonly NotesAppDatabaseContext _dbContext;

        /// <summary>
        /// Entity Framework settings for the application.
        /// </summary>
        private readonly NoteSettings _efSettings;

        public EntityFrameworNoteDataAccessLayer(MyOpenAiClient aiClient, TelemetryClient telemetryClient, NotesAppDatabaseContext dbContext, NoteSettings efSettings)
        {
            _aiClient = aiClient;
            _metricTracker = new NotesMetricTracker(telemetryClient);
            _dbContext = dbContext;
            _efSettings = efSettings;
        }

        /// <summary>
        /// Retrieves note by identifier.
        /// </summary>
        /// <param name="noteId">Note id</param>
        /// <returns>return note if found. Returns null if not found.</returns>
        public async Task<DtoNote?> GetNote(Guid noteId)
        {
            var note = await _dbContext.Note
                .AsNoTracking()
                .Where(Note => Note.NoteId == noteId)
                .Select(n => new DtoNote
                {
                    NoteId = n.NoteId,
                    Summary = n.Summary,
                    Details = n.Details,
                    CreatedDateUtc = n.CreatedDateUtc,
                    ModifiedDateUtc = n.ModifiedDateUtc,
                    Tags = n.Tags != null ? n.Tags.Select(tag => tag.Name).ToList() : new List<string>()
                })
                .FirstOrDefaultAsync();

            return note;
        }

        /// <summary>
        /// Retrieves all notes.
        /// </summary>
        /// <returns>Returns list of notes. If not notes, list will be empty.</returns>
        public async Task<List<DtoNote>> GetNotes(NoteSearchParameters parameters)
        {
            IQueryable<Note> query = _dbContext.Note;

            // Add search parameters
            if (!string.IsNullOrEmpty(parameters.TagName))
            {
                // Filter notes by tag name
                query = query.Where(Note => Note.Tags != null && Note.Tags.Any(Tag => Tag.Name == parameters.TagName));
            }

            var notes = await query
                .AsNoTracking()
                .Select(n => new DtoNote
                {
                    NoteId = n.NoteId,
                    Summary = n.Summary,
                    Details = n.Details,
                    CreatedDateUtc = n.CreatedDateUtc,
                    ModifiedDateUtc = n.ModifiedDateUtc,
                    Tags = n.Tags != null ? n.Tags.Select(tag => tag.Name).ToList() : new List<string>()
                })
                .ToListAsync();

            return notes;
        }

        /// <summary>
        /// Creates a note. Uses details and summary to crate note. Will generate id and tags AND set modified and created dates.
        /// </summary>
        /// <param name="newNote">Note to create</param>
        /// <exception cref="LimitReachedCustomException">Thrown if max notes limit is reached.</exception>"
        /// <returns>returns created note</returns>
        public async Task<DtoNote> CreateNote(DtoNote newNote)
        {
            var stopwatch = Stopwatch.StartNew();
            
            try
            {
                var noteCount = await _dbContext.Note.CountAsync();

                if (noteCount >= _efSettings.MaxNotes)
                {
                    throw new LimitReachedCustomException($"Max notes limit reached. Maximum allowed notes: [{_efSettings.MaxNotes}])");
                }

                if (string.IsNullOrEmpty(newNote.Summary) || string.IsNullOrEmpty(newNote.Details))
                {
                    throw new ArgumentException("Summary and details are required to create a note.");
                }

                // Add fields to note.
                var addNote = new Note
                {
                    NoteId = Guid.NewGuid(),
                    Summary = newNote.Summary,
                    Details = newNote.Details,
                    CreatedDateUtc = DateTime.UtcNow,
                    ModifiedDateUtc = null,
                    Tags = new List<Tag>()
                };

                // Get tags from OpenAI
                addNote.Tags = await _aiClient.RequestTagCollection(addNote, 30);

                // Save note to database
                _dbContext.Note.Add(addNote);
                await _dbContext.SaveChangesAsync();

                return new DtoNote(addNote);
            }
            catch
            {
                throw;
            }
            finally
            {
                stopwatch.Stop();

                _metricTracker.TrackNoteCreated(newNote, stopwatch.ElapsedMilliseconds);
            }
        }

        /// <summary>
        /// Updates a note.
        /// </summary>
        /// <param name="updatedNote">Note to update</param>
        /// <exception cref="NotFoundCustomException">Thrown if note cannot be found by id.</exception>""
        /// <returns>Returns true if note was updated, returns false if cannot find notes.</returns>
        public async Task UpdateNote(DtoNote updatedNote)
        {
            var stopwatch = Stopwatch.StartNew();           
            var updateMade = false;
            var trackPropeties = new Dictionary<string, string>();
            var trackMetrics = new Dictionary<string, double>();

            try
            {
                var existingNote = await _dbContext.Note
                    .Where(Note => Note.NoteId == updatedNote.NoteId)
                    .Include(Note => Note.Tags)
                    .FirstOrDefaultAsync();

                if (existingNote == null)
                {
                    throw new NotFoundCustomException($"Note not found by id: [{updatedNote.NoteId}]");
                }

                // If the summary is not empty, update the summary.
                if (!String.IsNullOrEmpty(updatedNote.Summary))
                {
                    updateMade = true;
                    existingNote.Summary = updatedNote.Summary;
                    trackPropeties.Add("Summary", updatedNote.Summary);
                    trackMetrics.Add("SummaryLength", updatedNote.Summary.Length);
                }
                else
                {
                    trackPropeties.Add("Summary", "");
                    trackMetrics.Add("SummaryLength", 0);
                }

                // If the details are not empty, update the details.
                if (!String.IsNullOrEmpty(updatedNote.Details))
                {
                    updateMade = true;
                    existingNote.Details = updatedNote.Details;
                    existingNote.Tags = await _aiClient.RequestTagCollection(existingNote, 30);
                    trackPropeties.Add("Details", updatedNote.Details);
                    trackMetrics.Add("DetailsLength", updatedNote.Details.Length);
                    trackMetrics.Add("TagCount", existingNote.Tags?.Count ?? 0);
                }
                else
                {
                    trackPropeties.Add("Details", "");
                    trackMetrics.Add("DetailsLength", 0);
                    trackMetrics.Add("TagCount", 0);
                }

                if (updateMade)
                {
                    // If an update was made, update the modified date and save changes.
                    existingNote.ModifiedDateUtc = DateTime.UtcNow;

                    // Save changes to database
                    _dbContext.Note.Update(existingNote);
                    await _dbContext.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                trackPropeties.Add("ExceptionThrown", ex.Message);

                throw;
            }
            finally
            {
                stopwatch.Stop();
                trackMetrics.Add("UpdateElapsedMilliseconds", stopwatch.ElapsedMilliseconds);

                _metricTracker.TrackNoteUpdated(trackPropeties, trackMetrics);
            }
        }
        /// <summary>
        /// Deletes a note by identifier.
        /// </summary>
        /// <param name="noteId">Id of note to delete</param>
        /// <exception cref="NotFoundCustomException">Thrown if note cannot be found by id.</exception>"
        /// <returns>Returns true if note was found and deleted.  Returns false if note was not found by id</returns>
        public async Task DeleteNote(Guid noteId)
        {
            var note = await _dbContext.Note
                .Where(Note => Note.NoteId == noteId)
                .Include(n => n.Tags)
                .FirstOrDefaultAsync();
           
            if (note == null)
            {
                throw new NotFoundCustomException($"Note not found by id: [{noteId}]");
            }

            // Remove tags
            // NOTE:  if there is a failure in deletion of the tags you shall delete the note or the attachments and log an error with the details of the failure
            if (note.Tags != null && note.Tags.Any())
            {
                _dbContext.Tag.RemoveRange(note.Tags);
            }

            // Remove note from database
            _dbContext.Note.Remove(note);
            _dbContext.SaveChanges();
        }

        /// <summary>
        /// Gets all distincttags from the data store.
        /// </summary>
        /// <returns>Returns all tags.</returns>
        public async Task<List<DtoTag>> GetUniqueTags()
        {
            var tags = await _dbContext.Tag
                .AsNoTracking()
                .Select(tag => new DtoTag(tag))
                .Distinct()
                .ToListAsync();

            return tags;
        }
    }
}
