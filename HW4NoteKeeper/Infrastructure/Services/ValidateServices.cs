using HW4NoteKeeperEx2.Data;
using HW4NoteKeeperEx2.Interfaces;

namespace HW4NoteKeeperEx2.Infrastructure.Services
{
    /// <summary>
    /// Validate services in the application
    /// 
    /// JB 4/24 Note: May be removed.  Was helpful for debugging but not needed in production.
    /// </summary>
    public static class ValidateServices
    {
        /// <summary>
        /// Validate a set of services in the application.
        /// </summary>
        /// <param name="app"></param>
        public static void Validate(WebApplication app)
        {
            try
            {
                using (var serviceScope = app.Services.CreateScope())
                {
                    Console.WriteLine("Resolving NotesAppDatabaseContext...");
                    var context = serviceScope.ServiceProvider.GetRequiredService<NotesAppDatabaseContext>();

                    Console.WriteLine("Resolving MyOpenAiClient...");
                    var aiClient = serviceScope.ServiceProvider.GetRequiredService<MyOpenAiClient>();

                    Console.WriteLine("Resolving IAzureStorageDataAccessLayer...");
                    var azStorage = serviceScope.ServiceProvider.GetRequiredService<IAzureStorageDataAccessLayer>();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error resolving services: {ex.Message}");
                throw;
            }
        }
    }
}
