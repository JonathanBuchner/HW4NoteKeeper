using HW4NoteKeeperEx2.DataAccessLayer;
using HW4NoteKeeperEx2.Infrastructure.Settings;
using HW4NoteKeeperEx2.Interfaces;
using Microsoft.Extensions.Options;

namespace HW4NoteKeeperEx2.Infrastructure.Services
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

            if (string.IsNullOrEmpty(settings.QueueNameFunction))
            {
                throw new ArgumentException("Queue name for function is required.");
            }

            if (string.IsNullOrEmpty(settings.QueueNameWebJob))
            {
                throw new ArgumentException("Queue name for webjob is required.");
            }
        }

    }
}
