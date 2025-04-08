namespace HW4NoteKeeper.Infrastructure.Settings
{
    /// <summary>
    /// Settings for the AI model
    /// </summary>
    public class AiModelSettings
    {
        /// <summary>
        /// API endpoint URI for the AI model.
        /// </summary>
        public string EndpointUri { get; set; } = "";

        /// <summary>
        /// API key for authentication with the AI model.
        /// </summary>
        public string ApiKey { get; set; } = "";

        /// <summary>
        /// The name of the AI model to be used.
        /// </summary>
        public string DeploymentModelName { get; set; } = "gpt-4o-mini";

        /// <summary>
        /// Maximum number of tokens to be used in the input prompt.
        /// </summary>
        public int MaxOutputTokens { get; set; } = 500;
    }
}
