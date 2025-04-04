using HW4NoteKeeper.Interfaces;
using HW4NoteKeeper.Models;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Metrics;
using Microsoft.IdentityModel.Abstractions;

namespace HW4NoteKeeper.MetricTrackers
{
    /// <summary>
    /// Metric tracker for notes controller.
    /// </summary>
    public class NotesMetricTracker : INotesMetricTracker
    {
        /// <summary>
        /// Application insights telemetry client.
        /// </summary>
        private TelemetryClient _telemetry;

        public NotesMetricTracker(TelemetryClient telemetry) 
        {
            _telemetry = telemetry; 
        }

        /// <summary>
        /// Track note created event through application insights.
        /// </summary>
        /// <param name="note">Note for trackign information</param>
        /// <param name="elapsedTimeInMilliseconds">Elapsed time in milliseconds</param>
        public void TrackNoteCreated(DtoNote note, long elapsedTimeInMilliseconds)
        {
            if (note == null)
            {
                throw new ArgumentNullException("Cannot track created note if note is null.");
            }

            if (note.Summary == null)
            {
                throw new ArgumentNullException("Cannot track create a note with a null summary.");
            }

            if (note.Details == null)
            {
                throw new ArgumentNullException("Cannot track created note with null details.");
            }

            var properties = new Dictionary<string, string>()
            {
                { "summary", note.Summary },
                { "tagcount", note.Tags?.Count.ToString() ?? "0" }
            };
            var metrics = new Dictionary<string, double>()
            {
                { "ElapsedTime", elapsedTimeInMilliseconds },
                { "SummaryLength", note.Summary.Length },
                { "DetailsLength", note.Details.Length }
            };

            TrackEvent("NoteCreated", properties, metrics);
        }

        /// <summary>
        /// Track note updated event through application insights.
        /// </summary>
        /// <param name="properties">Dictionary of properties to track.</param>
        /// <param name="metrics">Dictionary of metrics to track.</param>
        /// <remarks>Only tracks update to fields that were changed.</remarks>
        public void TrackNoteUpdated(Dictionary<string, string> properties, Dictionary<string, double> metrics)
        {
            TrackEvent("NoteUpdated", properties, metrics);
        }

        /// <summary>
        /// Tracks event through application insights.
        /// </summary>
        /// <param name="EventName">Event name</param>
        /// <param name="properties">Dictionary of properties</param>
        /// <param name="metrics">Dictionary of metrics</param>
        private void TrackEvent(string EventName, Dictionary<string, string> properties, Dictionary<string, double> metrics)
        {
            _telemetry.TrackEvent(EventName,
                properties: properties,
                metrics: metrics);
        }
    }
}
