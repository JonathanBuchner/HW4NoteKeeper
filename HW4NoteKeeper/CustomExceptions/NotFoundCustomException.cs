namespace HW4NoteKeeperEx2.CustomExceptions
{
    /// <summary>
    /// Exception thrown when a resource is not found.
    /// </summary>
    public class NotFoundCustomException : HW4NoteKeeperCustomException
    {
        public NotFoundCustomException(string message) : base(message) { }
    }
}
