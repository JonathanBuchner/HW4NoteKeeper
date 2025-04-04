namespace HW4NoteKeeper.Infrastructure.Settings
{
    /// <summary>
    /// Blob settings. Used when instantiating a blob client.
    /// </summary>
    public class AzureStorageDataAccessLayerSettings
    {
        /// <summary>
        /// Connection string for blob storage.
        /// </summary>
        public string ConnectionString { get; set; } = ""; // Must be overriden in secrets.json

        /// <summary>
        /// The maximum number of attachments that can be uploaded.
        /// </summary>
        public int MaxAttachments { get; set; } = 3;
    }
}
