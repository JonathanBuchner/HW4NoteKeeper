namespace HW4NoteKeeper.CustomExceptions
{
    /// <summary>
    /// Exception thrown when a limit is reached.
    /// </summary>
    public class LimitReachedCustomException : HW4NoteKeeperCustomException
    {
        public LimitReachedCustomException(string message) : base(message) { }
    }
}
