using HW4NoteKeeper.Models;

namespace HW4NoteKeeper.Interfaces
{
    public interface IAttachmentValidator
    {
        List<string> ValidateDto(DtoAttachment attachment);
    }
}
