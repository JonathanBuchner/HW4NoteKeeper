using HW4NoteKeeperEx2.DataAccessLayer;
using HW4NoteKeeperEx2.Infrastructure.Settings;
using HW4NoteKeeperEx2.Interfaces;
using Microsoft.Extensions.Options;

namespace HW4NoteKeeperEx2.Infrastructure.Services
{
    /// <summary>
    /// Register blob storage service
    /// </summary>
    public class RegistererBlobStorageService
    {
        /// <summary>
        /// Register the blob storage service with the DI container
        /// </summary>
        /// <param name="builder">builder</param>
        public static void AddBlobStorageService(WebApplicationBuilder builder)
        {
            builder.Services.Configure<AzureStorageDataAccessLayerSettings>(
                builder.Configuration.GetSection(nameof(AzureStorageDataAccessLayerSettings)));

            // Register the service with settings resolved from IOptions
            builder.Services.AddSingleton<IAzureStorageDataAccessLayer>(sp =>
            {
                var settings = sp.GetRequiredService<IOptions<AzureStorageDataAccessLayerSettings>>().Value;
                ValidateBlobSettings(settings);
                return new AzureStorageDataAccessLayer(settings);
            });
        }

        /// <summary>
        /// Validate the blob settings.
        /// </summary>
        /// <param name="blobSettings">blob settings</param>
        /// <exception cref="ArgumentException">Thrown if connecting string is empty</exception>
        private static void ValidateBlobSettings(AzureStorageDataAccessLayerSettings blobSettings)
        {
            if (string.IsNullOrEmpty(blobSettings.ConnectionString))
            {
                throw new ArgumentException("BlobSettings.ConnectionString is required.");
            }
        }
    }
}
