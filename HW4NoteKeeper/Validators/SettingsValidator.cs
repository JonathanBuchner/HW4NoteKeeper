using HW4NoteKeeperEx2.Infrastructure.Settings;

namespace HW4NoteKeeperEx2.Tools.Validators
{
    /// <summary>
    /// Validates the settings for the application.
    /// </summary>
    public static class SettingsValidator
    {
        /// <summary>
        /// Validates the AI settings.
        /// </summary>
        /// <param name="settings">The AI setting to validate</param>
        /// <exception cref="ArgumentNullException">Thrown if settings is null</exception>
        /// <exception cref="ArgumentException">Thrown if setting is missing</exception>
        public static void ValidateAiSettings(AiModelSettings settings)
        {
            if (settings == null)
            {
                throw new ArgumentNullException(nameof(settings));
            }
            
            if (string.IsNullOrWhiteSpace(settings.EndpointUri))
            {
                throw new ArgumentException("Endpoint URI is required.");
            }
            
            if (string.IsNullOrWhiteSpace(settings.ApiKey))
            {
                throw new ArgumentException("API Key is required.");
            }
            
            if (string.IsNullOrWhiteSpace(settings.DeploymentModelName))
            {
                throw new ArgumentException("Deployment Model Name is required.");
            }
            
            if (settings.MaxOutputTokens <= 0)
            {
                throw new ArgumentException("Max Output Tokens must be greater than 0.");
            }
        }

        /// <summary>
        /// Validates the Application Insights settings.
        /// </summary>
        /// <param name="settings"></param>
        /// <exception cref="ArgumentException"></exception>
        public static void ValidateApplicationInsightSettings(ApplicationInsightsSettings settings)
        {
            if (settings == null)
            {
                throw new ArgumentException(nameof(settings));
            }

            if (string.IsNullOrEmpty(settings.ApiKey))
            {
                throw new ArgumentException("API key is required for application insights");
            }
        }
    }
}
