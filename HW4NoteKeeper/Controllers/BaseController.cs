using HW4NoteKeeper.Dal;
using HW4NoteKeeper.Data;
using HW4NoteKeeper.DataAccessLayer;
using HW4NoteKeeper.Infrastructure.Services;
using HW4NoteKeeper.Infrastructure.Settings;
using HW4NoteKeeper.Interfaces;
using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Mvc;

namespace HW4NoteKeeper.Controllers
{
    /// <summary>
    /// Base controller for the application.
    /// </summary>
    public abstract class BaseController: ControllerBase
    {
        /// <summary>
        /// Note data access layer for saving notes to the database.
        /// </summary>
        protected readonly INoteDataAccessLayer _noteRepository;
        protected readonly IAzureStorageDataAccessLayer _azureStorageDataAccessLayer;

        public BaseController(
            MyOpenAiClient aiClient, 
            TelemetryClient telemetryClient, 
            NotesAppDatabaseContext dbContext, 
            NoteSettings efSettings,
            IAzureStorageDataAccessLayer azureStorageDataAccessLayer)
        {
            _noteRepository = new EntityFrameworNoteDataAccessLayer(aiClient, telemetryClient, dbContext, efSettings);
            _azureStorageDataAccessLayer = azureStorageDataAccessLayer;
        }

        #region Success 200 Responses
        /// <summary>
        /// Creates a custom response that logs additional data for a successful request.
        /// </summary>
        /// <typeparam name="T">Payload type</typeparam>
        /// <param name="tracker">Logger</param>
        /// <param name="title">title of log information</param>
        /// <param name="details">details of log information</param>
        /// <param name="additionalData">Additional information</param>
        /// <returns>Ok response</returns>
        protected IActionResult OkLogCustomResponse<T>(IApplicationLogger<T> tracker, string title, string details, Dictionary<string, object>? additionalData = null)
        {
            return OkLogCustom200Response(tracker, title, details, default, additionalData);
        }

        /// <summary>
        /// Creates a custom response that logs payload and additional data for a successful request.
        /// </summary>
        /// <typeparam name="T">Payload type</typeparam>
        /// <param name="tracker">Logger</param>
        /// <param name="title">title of log information</param>
        /// <param name="details">details of log information</param>
        /// <param name="payload">Payload to log</param>
        /// <param name="additionalData">Additional information</param>
        /// <returns>Ok response</returns>
        protected IActionResult OkLogCustom200Response<T>(IApplicationLogger<T> tracker, string title, string details, T? payload, Dictionary<string, object>? additionalData = null)
        {
            LogInformationCustomResponse(tracker, title, StatusCodes.Status200OK, details, payload, additionalData);

            return Ok();
        }

        /// <summary>
        /// Creates a custom response that additional data for a successful request.
        /// </summary>
        /// <typeparam name="T">Payload type</typeparam>
        /// <param name="tracker">Logger</param>
        /// <param name="title">title of log information</param>
        /// <param name="details">details of log information</param>
        /// <param name="additionalData">Additional information</param>
        /// <returns>Created response</returns>
        protected IActionResult CreatedLogCustomReponse<T>(IApplicationLogger<T> tracker, string title, string details, Dictionary<string, object>? additionalData = null)
        {
            return CreatedLogCustomReponse(tracker, title, details, default, additionalData);
        }

        /// <summary>
        /// Creates a custom response that logs payload and additional data for a successful request.
        /// </summary>
        /// <typeparam name="T">Payload type</typeparam>
        /// <param name="tracker">Logger</param>
        /// <param name="title">title of log information</param>
        /// <param name="details">details of log information</param>
        /// <param name="payload">Payload to log</param>
        /// <param name="additionalData">Additional information</param>
        /// <returns>Created response</returns>
        protected IActionResult CreatedLogCustomReponse<T>(IApplicationLogger<T> tracker, string title, string details, T? payload, Dictionary<string, object>? additionalData = null)
        {
            LogInformationCustomResponse(tracker, title, StatusCodes.Status201Created, details, payload, additionalData);

            return Created();
        }

        /// <summary>
        /// Creates a custom response that logs payload and additional data for a successful request.
        /// </summary>
        /// <typeparam name="T">Payload type</typeparam>
        /// <param name="tracker">Logger</param>
        /// <param name="title">title of log information</param>
        /// <param name="details">details of log information</param>
        /// <param name="additionalData">Additional information</param>
        /// <returns>No content response</returns>
        protected IActionResult NoContentLogCustomResponse<T>(IApplicationLogger<T> tracker, string title, string details, Dictionary<string, object>? additionalData = null)
        {
            return NoContentLogCustomResponse(tracker, title, details, default, additionalData);
        }

        /// <summary>
        /// Creates a custom response that logs payload and additional data for a successful request.
        /// </summary>
        /// <typeparam name="T">Payload type</typeparam>
        /// <param name="tracker">Logger</param>
        /// <param name="title">title of log information</param>
        /// <param name="details">details of log information</param>
        /// <param name="payload">Payload to log</param>
        /// <param name="additionalData">Additional information</param>
        /// <returns>No content response</returns>
        protected IActionResult NoContentLogCustomResponse<T>(IApplicationLogger<T> tracker, string title, string details, T? payload, Dictionary<string, object>? additionalData = null)
        {
            LogInformationCustomResponse(tracker, title, StatusCodes.Status204NoContent, details, payload, additionalData);

            return NoContent();
        }

        /// <summary>
        /// Creates a custom response that logs warning with additional data for a successful request.
        /// </summary>
        /// <typeparam name="T">Payload type</typeparam>
        /// <param name="tracker">Logger</param>
        /// <param name="title">title of log information</param>
        /// <param name="details">details of log information</param>
        /// <param name="additionalData">Additional information</param>
        /// <returns>No content response</returns>
        protected IActionResult NoContentWarnCustomResponse<T>(IApplicationLogger<T> tracker, string title, string details, Dictionary<string, object>? additionalData = null)
        {
            return NoContentWarnCustomResponse(tracker, title, details, default, additionalData);
        }


        /// <summary>
        /// Creates a custom response that logs warning with additional data for a successful request.
        /// </summary>
        /// <typeparam name="T">Payload type</typeparam>
        /// <param name="tracker">Logger</param>
        /// <param name="title">title of log information</param>
        /// <param name="details">details of log information</param>
        /// <param name="payload">Payload to log</param>
        /// <param name="additionalData">Additional information</param>
        /// <returns>No content response</returns>
        protected IActionResult NoContentWarnCustomResponse<T>(IApplicationLogger<T> tracker, string title, string details, T? payload, Dictionary<string, object>? additionalData = null)
        {
            LogWarnCustomResponse(tracker, title, StatusCodes.Status204NoContent, details, payload, additionalData);

            return NoContent();
        }

        #endregion

        #region Client 400 Responses

        /// <summary>
        /// Creates a custom response with ProblemDetails for a bad request (no exception).
        /// </summary>
        /// <typeparam name="T">Object for tracker</typeparam>
        /// <param name="tracker">Aplication insights tracker</param>
        /// <param name="title">Title for problem details</param>
        /// <param name="details">Details for problem details</param>
        /// <param name="additionalData">Optional additional data</param>
        /// <returns></returns>
        protected ObjectResult BadRequestCustomResponse<T>(IApplicationLogger<T> tracker, string title, string details, Dictionary<string, object>? additionalData = null)
        {
            return BadRequestCustomResponse(tracker, title, details, default, additionalData);
        }

        /// <summary>
        /// Creates a custom response with ProblemDetails for a bad request (no excepton).
        /// </summary>
        /// <typeparam name="T">Object for tracker</typeparam>
        /// <param name="tracker">Aplication insights tracker</param>
        /// <param name="title">Title for problem details</param>
        /// <param name="details">Details for problem details</param>
        /// <param name="payload">Optional Object that caused error</param>
        /// <param name="additionalData">Optional additional data</param>
        /// <returns></returns>
        protected ObjectResult BadRequestCustomResponse<T>(IApplicationLogger<T> tracker, string title, string details, T? payload, Dictionary<string, object>? additionalData = null)
        {
            return DefaultClientCustomResponse(tracker, title, StatusCodes.Status400BadRequest, details, payload, additionalData);
        }

        /// <summary>
        /// Creates a custom response with ProblemDetails for a bad request exception.
        /// </summary>
        /// <typeparam name="T">Object type for tracker</typeparam>
        /// <param name="tracker">Application insights tracker</param>
        /// <param name="payload">payload sent on request</param>
        /// <param name="ex">exception</param>
        /// <param name="title">title for the problem details</param>
        /// <returns>Object for the response that includes status code and problem details</returns>>
        protected ObjectResult BadRequestExceptionCustomResponse<T>(IApplicationLogger<T> tracker, T payload, Exception ex, string title)
        {
            return DefualtExceptionCustomResponse(tracker, payload, ex, title, StatusCodes.Status400BadRequest);
        }


        /// <summary>
        /// Creates a custom response with ProblemDetails for a forbidden (non-exception).
        /// </summary>
        /// <typeparam name="T">Object for tracker</typeparam>
        /// <param name="tracker">Aplication insights tracker</param>
        /// <param name="title">Title for problem details</param>
        /// <param name="details">Details for problem details</param>
        /// <param name="additionalData">Optional additional data</param>
        /// <returns>Object for the response that includes status code and problem details</returns>
        protected ObjectResult ForbiddenCustomResponse<T>(IApplicationLogger<T> tracker, string title, string details, Dictionary<string, object>? additionalData = null)
        {
            return ForbiddenCustomResponse(tracker, title, details, default, additionalData);
        }

        /// <summary>
        /// Creates a custom response with ProblemDetails for a forbidden (non-exception).
        /// </summary>
        /// <typeparam name="T">Object for tracker</typeparam>
        /// <param name="tracker">Aplication insights tracker</param>
        /// <param name="title">Title for problem details</param>
        /// <param name="details">Details for problem details</param>
        /// <param name="payload">Optional Object that caused error</param>
        /// <param name="additionalData">Optional additional data</param>
        /// <returns>Object for the response that includes status code and problem details</returns>
        protected ObjectResult ForbiddenCustomResponse<T>(IApplicationLogger<T> tracker, string title, string details, T? payload, Dictionary<string, object>? additionalData = null)
        {
            return DefaultClientCustomResponse(tracker, title, StatusCodes.Status403Forbidden, details, payload, additionalData);
        }

        /// <summary>
        /// Creates a custom response with ProblemDetails for a forbidden exception.
        /// </summary>
        /// <typeparam name="T">Object type for tracker</typeparam>
        /// <param name="tracker">Application insights tracker</param>
        /// <param name="payload">payload sent on request</param>
        /// <param name="ex">exception</param>
        /// <param name="title">title for the problem details</param>
        /// <returns>Object for the response that includes status code and problem details</returns>
        protected ObjectResult ForbiddenExceptionCustomResponse<T>(IApplicationLogger<T> tracker, T payload, Exception ex, string title)
        {
            return DefualtExceptionCustomResponse(tracker, payload, ex, title, StatusCodes.Status403Forbidden);
        }

        /// <summary>
        /// Creates a custom response with ProblemDetails for a not found (non exception).
        /// </summary>
        /// <typeparam name="T">Object for tracker</typeparam>
        /// <param name="tracker">Aplication insights tracker</param>
        /// <param name="title">Title for problem details</param>
        /// <param name="details">Details for problem details</param>
        /// <param name="payload">Optional Object that caused error</param>
        /// <param name="additionalData">Optional additional data</param>
        /// <returns>Object for the response that includes status code and problem details</returns>
        protected ObjectResult NotFoundCustomResponse<T>(IApplicationLogger<T> tracker, string title, string details, Dictionary<string, object>? additionalData = null)
        {
            return NotFoundCustomResponse(tracker, title, details, default, additionalData);
        }

        /// <summary>
        /// Creates a custom response with ProblemDetails for a not found (non exception).
        /// </summary>
        /// <typeparam name="T">Object for tracker</typeparam>
        /// <param name="tracker">Aplication insights tracker</param>
        /// <param name="title">Title for problem details</param>
        /// <param name="details">Details for problem details</param>
        /// <param name="payload">Optional Object that caused error</param>
        /// <param name="additionalData">Optional additional data</param>
        /// <returns>Object for the response that includes status code and problem details</returns>
        protected ObjectResult NotFoundCustomResponse<T>(IApplicationLogger<T> tracker, string title, string details, T? payload, Dictionary<string, object>? additionalData = null)
        {
            return DefaultClientCustomResponse(tracker, title, StatusCodes.Status404NotFound, details, payload, additionalData);
        }

        /// <summary>
        /// Creates a custom response with ProblemDetails for a not found exception.
        /// </summary>
        /// <typeparam name="T">Object type for tracker</typeparam>
        /// <param name="tracker">Application insights tracker</param>
        /// <param name="payload">payload sent on request</param>
        /// <param name="ex">exception</param>
        /// <param name="title">title for the problem details</param>
        /// <returns>Object for the response that includes status code and problem details</returns>
        protected ObjectResult NotFoundExceptionCustomResponse<T>(IApplicationLogger<T> tracker, T payload, Exception ex, string title)
        {
            return DefualtExceptionCustomResponse(tracker, payload, ex, title, StatusCodes.Status404NotFound);
        }

        #endregion

        #region Server 500 Responses

        /// <summary>
        /// Creates a custom response with ProblemDetails for an internal server error for a reuqest with a payload.
        /// </summary>
        /// <typeparam name="T">Object type for tracker</typeparam>
        /// <param name="tracker">Application insights tracker</param>
        /// <param name="payload">payload sent on request</param>_noteRepository
        /// <param name="ex">exception</param>
        /// <returns>Object for the response that includes status code and problem details</returns>
        protected ObjectResult InternalServerErrorExceptionCustomResponse<T>(IApplicationLogger<T> tracker, T payload, Exception ex)
        {
            return DefualtExceptionCustomResponse(tracker, payload, ex, "Internal Server Error", StatusCodes.Status500InternalServerError);
        }

        /// <summary>
        /// Creates a custom response with ProblemDetails for an internal server error exception for a request without a payload.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="tracker"></param>
        /// <param name="ex"></param>
        /// <returns></returns>
        protected ObjectResult InternalServerErrorExceptionCustomResponse<T>(IApplicationLogger<T> tracker, Exception ex)
        {
            return DefualtExceptionCustomResponse(tracker, ex, "Internal Server Error", StatusCodes.Status500InternalServerError);
        }

        #endregion

        #region Default Responses

        /// <summary>
        /// Defualt a custom response with ProblemDetails to be inherited by other exception responses. Includes payload.
        /// </summary>
        /// <typeparam name="T">Object type for tracker</typeparam>
        /// <param name="tracker">Application insights tracker</param>
        /// <param name="payload">payload sent on request</param>
        /// <param name="ex">exception</param>
        /// <param name="title">title for the problem details</param>
        /// <param name="statusCode">status code for the response</param>
        /// <returns>Object for the response that includes status code and problem details</returns>
        private ObjectResult DefualtExceptionCustomResponse<T>(IApplicationLogger<T> tracker, T payload, Exception ex, string title, int statusCode)
        {
            tracker.LogException(ex, payload);

            return DefaultExceptionCustomResponse(ex, title, statusCode);
        }

        /// <summary>
        /// Defualt a custom response with ProblemDetails to be inherited by other exception responses.  No payload.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="tracker"></param>
        /// <param name="ex"></param>
        /// <param name="title"></param>
        /// <param name="statusCode"></param>
        /// <returns></returns>
        private ObjectResult DefualtExceptionCustomResponse<T>(IApplicationLogger<T> tracker, Exception ex, string title, int statusCode)
        {
            tracker.LogException(ex);

            return DefaultExceptionCustomResponse(ex, title, statusCode);
        }

        /// <summary>
        ///  Defualt a custom response with ProblemDetails to be inherited by other exception responses.
        /// </summary>
        /// <param name="ex">exception</param>
        /// <param name="title">title for the problem details</param>
        /// <param name="statusCode">status code for the response</param>
        /// <returns>Object for the response that includes status code and problem details</returns>
        private ObjectResult DefaultExceptionCustomResponse(Exception ex, string title, int statusCode)
        {
            return DefaultClientServerProblemDetailsCustomResponse(title, statusCode, ex.Message);
        }

        /// <summary>
        /// Creates a custom response with ProblemDetails. Client reponse requests send this response and log as warnings.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="tracker"></param>
        /// <param name="title"></param>
        /// <param name="statusCode"></param>
        /// <param name="detail"></param>
        /// <param name="payload"></param>
        /// <param name="additionalData"></param>
        /// <returns></returns>
        private ObjectResult DefaultClientCustomResponse<T>(IApplicationLogger<T> tracker, string title, int statusCode, string detail, T? payload, Dictionary<string, object>? additionalData = null)
        {
            LogWarnCustomResponse(tracker, title, statusCode, detail, payload, additionalData);

            return DefaultClientServerProblemDetailsCustomResponse(title, statusCode, detail);
        }

        /// <summary>
        /// Creates a custom response with ProblemDetails. Exceptions and bad client/server requests send this response.
        /// </summary>
        /// <param name="title">Title of the reponse</param>
        /// <param name="statusCode">Status code</param>
        /// <param name="detail">details of message</param>
        /// <returns>returns problem details</returns>
        private ObjectResult DefaultClientServerProblemDetailsCustomResponse(string title, int statusCode, string detail)
        {
            var problemDetails = new ProblemDetails
            {
                Title = title,
                Status = statusCode,
                Detail = detail
            };

            return StatusCode(statusCode, problemDetails);
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Logs a successful response with custom payload and additional data.
        /// </summary>
        /// <typeparam name="T">Payload type</typeparam>
        /// <param name="tracker">tracker</param>
        /// <param name="title">message title</param>
        /// <param name="statusCode">status code</param>
        /// <param name="detail">details to log</param>
        /// <param name="payload">payload to log</param>
        /// <param name="additionalData">additional information to log</param>
        private void LogInformationCustomResponse<T>(IApplicationLogger<T> tracker, string title, int statusCode, string detail, T? payload, Dictionary<string, object>? additionalData = null)
        {
            if (payload is not null && additionalData is not null)
            {
                tracker.LogInformation(detail, payload, additionalData);
            }
            else if (payload is not null)
            {
                tracker.LogInformation(detail, payload);
            }
            else if (additionalData is not null)
            {
                tracker.LogInformation(detail, additionalData);
            }
            else
            {
                tracker.LogInformation(detail);
            }
        }

        /// <summary>
        /// Logs a warning response with custom payload and additional data.
        /// </summary>
        /// <typeparam name="T">Payload type</typeparam>
        /// <param name="tracker">tracker</param>
        /// <param name="title">message title</param>
        /// <param name="statusCode">status code</param>
        /// <param name="detail">details to log</param>
        /// <param name="payload">payload to log</param>
        /// <param name="additionalData">additional information to log</param>
        private void LogWarnCustomResponse<T>(IApplicationLogger<T> tracker, string title, int statusCode, string detail, T? payload, Dictionary<string, object>? additionalData = null)
        {
            if (payload is not null && additionalData is not null)
            {
                tracker.LogWarning(detail, payload, additionalData);
            }
            else if (payload is not null)
            {
                tracker.LogWarning(detail, payload);
            }
            else if (additionalData is not null)
            {
                tracker.LogWarning(detail, additionalData);
            }
            else
            {
                tracker.LogWarning(detail);
            }
        }

        #endregion
    }
}
