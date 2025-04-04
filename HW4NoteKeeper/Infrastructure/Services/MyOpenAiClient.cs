using System.ClientModel;
using System.ComponentModel.DataAnnotations;
using System.Numerics;
using Azure;
using Azure.AI.OpenAI;
using HW4NoteKeeper.Infrastructure.Settings;
using HW4NoteKeeper.Models;
using Microsoft.Extensions.AI;

namespace HW4NoteKeeper.Infrastructure.Services
{
    /// <summary>
    /// A class to interact with the OpenAI API on Azure
    /// </summary>
    public class MyOpenAiClient
    {
        /// <summary>
        /// The chat client used to interact with the OpenAI API.
        /// </summary>
        private readonly IChatClient _chatClient;

        /// <summary>
        /// The settings for the AI model.
        /// </summary>
        private readonly AiModelSettings _aiSettings;

        public MyOpenAiClient(IChatClient chatClient, AiModelSettings aiSettings)
        {
            _chatClient = chatClient;
            _aiSettings = aiSettings;
        }

        /// <summary>
        /// Request a response from the OpenAI API service.
        /// </summary>
        /// <param name="prompt">prompt used</param>
        /// <returns>Current an a prmompt enhancements suggests returning list of one to five tags summarizing the prompt</returns>
        public async Task<string> Request(string prompt)
        {
            var chatOptions = new ChatOptions()
            {
                MaxOutputTokens = _aiSettings.MaxOutputTokens
            };

            var promptEnhancement =
                   "This is a prompt enhancement. " +
                   "After the end of the prompt enhancement will be the details for a note." +
                   "Respond with two to five one word 'tags' seperated by commas (e.g. Washington, important, farming, technology, planting)." +
                   "These tags should summarize the details of the note. A longer note details like the lengtth of a paragraph should have five tags." +
                   "A sentence with only two words would probably have two tag. " +
                   "Again, please only provide one word 'tags' seperate by commas." +
                   "Do not write a reponse like, 'Sure! Please provide the details of the note for the tags.' Instead, try to provide a list of tags." +
                   "This ends the prompt enhancement. " + prompt;


            var response = await _chatClient.GetResponseAsync(chatMessage: promptEnhancement, options: chatOptions);

            if (response.Message.Text != null)
            {
                return response.Message.Text;
            }
            else
            {
                return "Unable to respond. Error: " + response.Message;
            }
        }

        /// <summary>
        /// Request a list of tags from the OpenAI API service.
        /// </summary>
        /// <param name="prompt">prompt to use to return tags.</param>
        /// <param name="maxLength">maximum length of a tag. Default is 1024 (tags are expected to be single words</param>
        /// <returns>List of strings created from prompt</returns>
        public async Task<ICollection<string>> RequestCollection(string prompt, int maxLength = 30)
        {
            var response = await Request(prompt);

            if (response != null)
            {
                var tags = response.Split(',')
                   .Select(tag => tag.Trim())
                   .Take(maxLength)
                   .Where(tag => !string.IsNullOrEmpty(tag))
                   .ToList();

                // This is an attempt to fix a bad response by the ai.  E.g. a response like, "Sure! Please provide the details of the note for the tags."
                if (tags.Count == 1)
                {
                    var noTagsGenerated = "No tags generated for note.";
                    noTagsGenerated = noTagsGenerated[..Math.Min(noTagsGenerated.Length, maxLength)]; // Check that no tags generated is less than max length.

                    return new List<string> { noTagsGenerated };
                }

                return tags;
            }
            else
            {
                return new List<string>();
            }
        }

        /// <summary>
        /// Request a collection of tags from the OpenAI API service.
        /// </summary>
        /// <param name="note">note to use to return tags.</param>
        /// <param name="maxLength">maximum length of a tag. Default is 1024 (tags are expected to be single words.</param>
        /// <returns>Collection of tags created from prompt.</returns>
        public async Task<ICollection<Tag>> RequestTagCollection(Note note, int maxLength)
        {
            var tags = new List<Tag>();
            var tagCollection = await RequestCollection(note.Details, maxLength);

            if (tagCollection == null)
            {
                return tags;
            }

            foreach (var tag in tagCollection)
            {
                tags.Add(new Tag
                {
                    Id = Guid.NewGuid(),
                    NoteId = note.NoteId,
                    Note = note,
                    Name = tag
                });
            }

            return tags;
        }
    }
}