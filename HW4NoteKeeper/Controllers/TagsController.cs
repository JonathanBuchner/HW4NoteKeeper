using HW4NoteKeeperEx2.ApplicationInsightsTrackers;
using HW4NoteKeeperEx2.Data;
using HW4NoteKeeperEx2.Infrastructure.Services;
using HW4NoteKeeperEx2.Infrastructure.Settings;
using HW4NoteKeeperEx2.Interfaces;
using HW4NoteKeeperEx2.Models;
using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Mvc;

namespace HW4NoteKeeperEx2.Controllers
{
    /// <summary>
    /// Controller for handling tags.  Follows rest conventions.
    /// </summary>
    [ApiController]
    [Route("[controller]")]
    public class TagsController : BaseController
    {
        /// <summary>
        /// Application insights exception tracker for tags.
        /// </summary>
        private readonly IApplicationLogger<Tag> _aiTagExceptionTracker;

        public TagsController(
            MyOpenAiClient aiClient, 
            TelemetryClient telemetryClient, 
            NotesAppDatabaseContext dbContext, 
            NoteSettings efSettings, 
            ILogger<TagsController> logger, 
            IAzureStorageDataAccessLayer azureStorageDataAccessLayer)
            : base(aiClient, telemetryClient, dbContext, efSettings, azureStorageDataAccessLayer) 
        {
            _aiTagExceptionTracker = new ApplicationLogger<Tag>(telemetryClient, logger);
        }

        // GET: TagsController/
        /// <summary>
        /// Retrieves all notes in the data store.  Will return 500 internal server error with ProblemDetails if exception occurs.
        /// </summary>
        /// <returns>Returns all notes.</returns>
        [HttpGet]
        [ProducesResponseType(typeof(List<DtoTag>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(void), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<List<DtoTag>>> GetAll()
        {
            try
            {
                var notes = await _noteRepository.GetUniqueTags();

                return Ok(notes);
            }

            catch (Exception ex)
            {
                return InternalServerErrorExceptionCustomResponse(_aiTagExceptionTracker, ex);
            }
        }
    }
}
