namespace HW4NoteKeeper.Enums
{
    /// <summary>
    /// Enum for the response of the PutAttachment method in the AzureStorageDataAccessLayer.
    /// </summary>
    public enum BlobStorageResponseUpdateCreate
    {
        Created,
        Updated,
        InternalServerError,
        RejectedMaxSizeExceeded,

    }

    /// <summary>
    /// Enum for the response of the DeleteAttachment method in the AzureStorageDataAccessLayer.
    /// </summary>
    public enum BlobStorageResponseDelete
    {
        Deleted,
        NotFound,
        InternalServerError
    }
}
