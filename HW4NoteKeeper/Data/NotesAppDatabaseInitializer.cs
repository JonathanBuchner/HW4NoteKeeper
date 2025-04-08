using System;
using System.Net.NetworkInformation;
using HW4NoteKeeper.Infrastructure.Services;
using HW4NoteKeeper.Interfaces;
using HW4NoteKeeper.Models;
using Microsoft.AspNetCore.StaticAssets;
using Microsoft.Identity.Client.Extensions.Msal;

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
        public static async Task InitilizeDbAsync(NotesAppDatabaseContext context, MyOpenAiClient aiClient, IAzureStorageDataAccessLayer azStorage)
        {
            if (context.Note.Any())
            {
                return;
            }

            // Create default notes
            var data = GetDefaultNotes();

            // Generate tags and add to dbcontext for each note.
            await AddNotesToDatabase(context, aiClient, data);

            // Add attachments to the notes
            await AddAttachmentToDatabase(azStorage, data);
        }

        /// <summary>
        /// Get default notes for the application.  This method is used to create default notes and tags for the notes.
        /// </summary>
        /// <returns>List of notes and file names to upload to blob storage</returns>
        private static List<(Note note, List<string> attachments)> GetDefaultNotes()
        {
            return new List<(Note note, List<string> attachments)>()
            {
                (
                    new Note() { NoteId = Guid.NewGuid(), Summary = "Running grocery list", Details = "Milk Eggs Oranges", CreatedDateUtc = DateTimeOffset.Now, ModifiedDateUtc = null, Tags = new List<Tag>() },
                    new List<string>() { "MilkAndEggs.png", "Oranges.png" }
                ),
                (
                    new Note() { NoteId = Guid.NewGuid(), Summary = "Gift supplies notes", Details = "Tape & Wrapping Paper", CreatedDateUtc = DateTimeOffset.Now, ModifiedDateUtc = null, Tags = new List<Tag>() },
                    new List<string>() { "Tape.png", "WrappingPaper.png" }
                ),
                (
                    new Note() { NoteId = Guid.NewGuid(), Summary = "Valentine's Day gift ideas", Details = "Chocolate, Diamonds, New Car", CreatedDateUtc = DateTimeOffset.Now, ModifiedDateUtc = null, Tags = new List<Tag>() },
                    new List<string>() { "Chocolate.png", "Diamonds.png", "NewCar.png", }
                ),
                (
                    new Note() { NoteId = Guid.NewGuid(), Summary = "Azure tips", Details = "portal.azure.com is a quick way to get to the portal Remember double ununderscore for linux and colon for windows", CreatedDateUtc = DateTimeOffset.Now, ModifiedDateUtc = null, Tags = new List<Tag>() },
                    new List<string>() { "AzureLogo.png", "AzureTipsAndTricks.pdf", }
                ),
            };
        }

        /// <summary>
        /// Add notes to the database.  This method is used to add notes to the database and generate tags for the notes.
        /// </summary>
        /// <param name="context">databse context</param>
        /// <param name="aiClient">ai client</param>
        /// <param name="intializedData">List of notes to add</param>
        /// <returns>Task</returns>
        private static async Task AddNotesToDatabase(NotesAppDatabaseContext context, MyOpenAiClient aiClient, List<(Note note, List<string> attachments)> intializedData)
        {
            foreach (var tuple in intializedData)
            {
                // Get tag list.  Tags are reduce to length of 30 characters when collection is returned.
                var tags = await aiClient.RequestCollection(tuple.note.Details, 30);
                tuple.note.Tags = new List<Tag>();                                    // Redandant, but just to be safe and remove warnings

                foreach (var tag in tags)
                {
                    if (tag == null || tag.Length <= 1)
                    {
                        continue;
                    }

                    var newTag = new Tag()
                    {
                        Id = Guid.NewGuid(),
                        NoteId = tuple.note.NoteId,
                        Note = tuple.note,
                        Name = tag
                    };

                    tuple.note.Tags.Add(newTag);
                }

                context.Note.Add(tuple.note);
            }

            await context.SaveChangesAsync();
        }

        /// <summary>
        /// Add attachments to the database.  This method is used to add attachments to the database and upload them to Azure storage.
        /// </summary>
        /// <param name="azStorage"></param>
        /// <param name="intializedData"></param>
        /// <returns></returns>
        private static async Task AddAttachmentToDatabase(IAzureStorageDataAccessLayer azStorage, List<(Note note, List<string> attachments)> intializedData)
        {
            foreach (var tuple in intializedData)
            {
                foreach (var attachment in tuple.attachments)
                {
                    // Base directory will be in bin, debug, net9.0...so in this case we need to go up three directories to get to the root of the project
                    var filePath = Path.Combine(AppContext.BaseDirectory, "../../../..", "SampleAttachments", attachment);
                    var absolutePath = Path.GetFullPath(filePath);

                    // Get file
                    await using var stream = new FileStream(absolutePath, FileMode.Open, FileAccess.Read);

                    var formFile = new FormFile(stream, 0, stream.Length, "file", Path.GetFileName(absolutePath))
                    {
                        Headers = new HeaderDictionary(),
                        ContentType = "application/octet-stream"
                    };

                    // Create a new attachment
                    var newAttachment = new Attachment()
                    {
                        NoteId = tuple.note.NoteId,
                        AttachmentId = attachment,
                        FileData = formFile,
                    };

                    // Upload the attachment to Azure storage
                    await azStorage.PutAttachment(newAttachment);
                }
            }
        }
    }
}
