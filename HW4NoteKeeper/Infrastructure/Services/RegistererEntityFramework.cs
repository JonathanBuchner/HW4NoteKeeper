using System.Threading.Tasks;
using Azure.Core;
using Azure.Identity;
using HW4NoteKeeper.Data;
using HW4NoteKeeper.Infrastructure.Settings;
using HW4NoteKeeper.Interfaces;
using HW4NoteKeeper.Validators;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;

namespace HW4NoteKeeper.Infrastructure.Services
{
    /// <summary>
    /// Handle stting up and and initializing db for entity framework
    /// </summary>
    public static class RegistererEntityFramework
    {
        /// <summary>
        /// Adds entity framework including connection to db and add entity framework settings
        /// </summary>
        /// <param name="builder">builder</param>
        public static void AddEntityFramework(WebApplicationBuilder builder)
        {
            // Add entity framework settings
            AddEntityFrameworkSettings(builder);

            // Configure Entity Framework to use SQL Server
            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? "";
            EntityFrameworkValidator.ValidateConnectionString(connectionString);

            builder.Services.AddDbContext<NotesAppDatabaseContext>(options =>
            {
                options.UseSqlServer(connectionString);
            });
        }

        private static void AddEntityFrameworkSettings(WebApplicationBuilder builder)
        {
            var NoteSettings = new NoteSettings();
            builder.Configuration.GetSection(nameof(NoteSettings)).Bind(NoteSettings);
            EntityFrameworkValidator.ValidateNoteSettings(NoteSettings);
            builder.Services.AddSingleton(implementationInstance: NoteSettings);
        }

        /// <summary>
        /// Initial seeding to database. 
        /// </summary>
        /// <param name="app">app</param>
        /// <remarks>Initialize db async does not use migrations as of 2/25 JB</remarks>
        /// <returns>Returns awaitable task</returns>
        public static async Task InitalizeDatabase(WebApplication app)
        {

            using (var serviceScope = app.Services.CreateScope())
            {
                var context = serviceScope.ServiceProvider.GetRequiredService<NotesAppDatabaseContext>();
                var aiClient = serviceScope.ServiceProvider.GetRequiredService<MyOpenAiClient>();
                var azStorage = serviceScope.ServiceProvider.GetRequiredService<IAzureStorageDataAccessLayer>();

                try
                {
                    await NotesAppDatabaseInitializer.InitilizeDbAsync(context, aiClient, azStorage);
                }
                catch (Exception ex)
                {
                    var logger = serviceScope.ServiceProvider.GetRequiredService<ILogger<Program>>();
                    logger.LogError(ex, "Failed to seed database.");

                }
            }
        }
    }
}
