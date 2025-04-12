using HW4NoteKeeper.DataAccessLayer;
using HW4NoteKeeper.Infrastructure.Settings;
using HW4NoteKeeper.Interfaces;
using Microsoft.Extensions.Options;

namespace HW4NoteKeeper.Infrastructure.Services
{
    /// <summary>
    /// Service for registering the ZipRequestQueueService.
    /// </summary>
    public static class RegistererZipRequestQueueService
    {
        /// <summary>
        /// Registers the ZipRequestQueueService with the DI container.
        /// </summary>
        /// <param name="builder">builder</param>
        public static void AddZipQueueService(WebApplicationBuilder builder)
        {
            builder.Services.Configure<ZipRequestQueueServiceSettings>(builder.Configuration.GetSection("ZipRequestQueueServiceSettings"));

            builder.Services.AddSingleton<IZipRequestQueueService>(sp =>
            {
                var settings = sp.GetRequiredService<IOptions<ZipRequestQueueServiceSettings>>().Value;
                ValidateSettings(settings);

                return new ZipRequestQueueService(settings);
            });
        }

        /// <summary>
        /// Validates the settings for the ZipRequestQueueService.
        /// </summary>
        /// <param name="settings">buldier</param>
        /// <exception cref="ArgumentException">Throws validation exception</exception>
        private static void ValidateSettings(ZipRequestQueueServiceSettings settings)
        {
            if (string.IsNullOrEmpty(settings.ConnectionString))
            {
                throw new ArgumentException("Connection string is required.");
            }

            if (string.IsNullOrEmpty(settings.QueueName))
            {
                throw new ArgumentException("Queue name is required.");
            }
        }

    }
}
