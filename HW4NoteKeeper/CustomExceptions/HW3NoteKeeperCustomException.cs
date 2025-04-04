namespace HW4NoteKeeper.CustomExceptions
{
    /// <summary>
    /// Custom exception for the HW4NoteKeeper application.  All other exceptions inherit from this exception.
    /// </summary>
    public class HW4NoteKeeperCustomException : Exception 
    {
        public HW4NoteKeeperCustomException(string message) : base(message) { }
    }
}
