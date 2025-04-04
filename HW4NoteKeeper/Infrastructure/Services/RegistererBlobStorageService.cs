using HW4NoteKeeper.DataAccessLayer;
using HW4NoteKeeper.Infrastructure.Settings;
using HW4NoteKeeper.Interfaces;

namespace HW4NoteKeeper.Infrastructure.Services
{
    public class RegistererBlobStorageService
    {
        public static void AddBlobStorageService(WebApplicationBuilder builder)
        {
            var settings = AddBlobStorageSettings(builder);

            builder.Services.AddSingleton<IAzureStorageDataAccessLayer>(serviceProvider => new AzureStorageDataAccessLayer(settings));
        }

        private static AzureStorageDataAccessLayerSettings AddBlobStorageSettings(WebApplicationBuilder builder)
        {
            var blobSettings = new AzureStorageDataAccessLayerSettings();
            builder.Configuration.GetSection(nameof(AzureStorageDataAccessLayerSettings)).Bind(blobSettings);
            ValidateBlobSettings(blobSettings);
            builder.Services.AddSingleton(implementationInstance: blobSettings);

            return blobSettings;
        }

        private static void ValidateBlobSettings(AzureStorageDataAccessLayerSettings blobSettings)
        {
            if (string.IsNullOrEmpty(blobSettings.ConnectionString))
            {
                throw new ArgumentException("BlobSettings.ConnectionString is required.");
            }
        }
    }
}
