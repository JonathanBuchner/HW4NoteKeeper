using HW4NoteKeeper.DataAccessLayer;
using HW4NoteKeeper.Infrastructure.Settings;
using HW4NoteKeeper.Interfaces;
using Microsoft.Extensions.Options;

namespace HW4NoteKeeper.Infrastructure.Services
{
    public static class RegistererZipRequestQueueService
    {
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
