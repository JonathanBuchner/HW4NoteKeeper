using HW4NoteKeeper.Infrastructure.Settings;

namespace HW4NoteKeeper.Validators
{
    /// <summary>
    /// Validator for Entity Framework service.
    /// </summary>
    public static class EntityFrameworkValidator
    {
        /// <summary>
        /// Validates the connection string. Used because connection string is required and new developer may not know this needs to be added to secrets.json.
        /// </summary>
        /// <param name="connectionString"></param>
        /// <exception cref="ArgumentException"></exception>
        public static void ValidateConnectionString(string connectionString)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new ArgumentException("Connection string is required.  Connection string was null or empty. You may need to update your secrete.json.");
            }
        }

        /// <summary>
        /// Validates the note settings.
        /// </summary>
        /// <param name="noteSettings"></param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException"></exception>
        public static void ValidateNoteSettings(NoteSettings noteSettings)
        {
            if (noteSettings == null)
            {
                throw new ArgumentNullException(nameof(noteSettings));
            }

            if (noteSettings.MaxNotes < 1)
            {
                throw new ArgumentException("MaxNotes in note settings must be greater than 0.");
            }
        }
    }
}
