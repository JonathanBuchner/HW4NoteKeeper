using HW4NoteKeeper;
using HW4NoteKeeperEx2.Infrastructure.Settings;
using HW4NoteKeeperEx2.Tools.Validators;

namespace HW4NoteKeeperEx2.Infrastructure.Services
{
    /// <summary>
    /// Configures the settings for the application.
    /// </summary>
    public static class RegistererConfigureSettings
    {
        /// <summary>
        /// Adds the configuration settings to the application.
        /// </summary>
        /// <param name="builder">builder</param>
        public static void AddConfiguration(WebApplicationBuilder builder)
        {
            builder.Configuration
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")}.json", optional: true)
                .AddUserSecrets<Program>()
                .AddEnvironmentVariables();
        }
        /// <summary>
        /// Adds the AI settings to the application.
        /// </summary>
        /// <param name="builder">builder</param>
        public static void AddAiSettings(WebApplicationBuilder builder)
        {
            AiModelSettings? settings = builder.Configuration.GetSection("AiModelSettings").Get<AiModelSettings>()!;
            SettingsValidator.ValidateAiSettings(settings);
            builder.Services.AddSingleton(settings);
        }
    }
}
