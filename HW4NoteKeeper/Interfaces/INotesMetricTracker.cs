using HW4NoteKeeperEx2.Models;

namespace HW4NoteKeeperEx2.Interfaces
{
    /// <summary>
    /// Interface for tracking metrics for notes.
    /// </summary>
    public interface INotesMetricTracker
    {
        /// <summary>
        /// Track note created event through application insights.
        /// </summary>
        /// <param name="note">Note for trackign information</param>
        /// <param name="elapsedTimeInMillseconds">Elapsed time in milliseconds</param>
        void TrackNoteCreated(DtoNote note, long elapsedTimeInMillseconds);

        /// <summary>
        /// Track note updated event through application insights.
        /// </summary>
        /// <param name="properties">Properties to track</param>
        /// <param name="events">Events to track</param>
        public void TrackNoteUpdated(Dictionary<string, string> properties, Dictionary<string, double> events);
    }
}
