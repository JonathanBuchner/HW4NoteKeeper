using System.Net.NetworkInformation;
using HW4NoteKeeper.Infrastructure.Services;
using HW4NoteKeeper.Models;

namespace HW4NoteKeeper.Data
{
    /// <summary>
    /// Database initializer for the application.  This class is used to initialize the database with default data and is a solution that does include using migratins
    /// </summary>
    public class NotesAppDatabaseInitializer
    {
        /// <summary>
        /// Initialize the database with default data.  This method is used to create default notes and tags for the notes.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="aiClient"></param>
        /// <returns></returns>
        public static async Task InitilizeDbAsync(NotesAppDatabaseContext context, MyOpenAiClient aiClient)
        {
            if (context.Note.Any())
            {
                return;
            }

            // Create default notes
            var notes = new List<Note>()
                {
                    new Note() { NoteId = Guid.NewGuid(), Summary = "Running grocery list", Details = "Milk Eggs Oranges", CreatedDateUtc = DateTimeOffset.Now, ModifiedDateUtc = null, Tags = new List<Tag>() },
                    new Note() { NoteId = Guid.NewGuid(), Summary = "Gift supplies notes", Details = "Tape & Wrapping Paper", CreatedDateUtc = DateTimeOffset.Now, ModifiedDateUtc = null, Tags = new List<Tag>() },
                    new Note() { NoteId = Guid.NewGuid(), Summary = "Valentine's Day gift ideas", Details = "Chocolate, Diamonds, New Car", CreatedDateUtc = DateTimeOffset.Now, ModifiedDateUtc = null, Tags = new List<Tag>() },
                    new Note() { NoteId = Guid.NewGuid(), Summary = "Azure tips", Details = "portal.azure.com is a quick way to get to the portal Remember double ununderscore for linux and colon for windows", CreatedDateUtc = DateTimeOffset.Now, ModifiedDateUtc = null, Tags = new List<Tag>() }
                };

            // Generate tags and add to dbcontext for each note.
            foreach (var note in notes)
            {
                // Get tag list.  Tags are reduce to length of 30 characters when collection is returned.
                var tags = await aiClient.RequestCollection(note.Details, 30);
                note.Tags = new List<Tag>();                                    // Redandant, but just to be safe and remove warnings

                foreach (var tag in tags)
                {
                    if (tag == null || tag.Length <= 1)
                    {
                        continue;
                    }

                    var newTag = new Tag()
                    {
                        Id = Guid.NewGuid(),
                        NoteId = note.NoteId,
                        Note = note,
                        Name = tag
                    };

                    note.Tags.Add(newTag);
                }

                context.Note.Add(note);
            }

            await context.SaveChangesAsync();
        }
    }
}
