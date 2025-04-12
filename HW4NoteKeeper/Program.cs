
using HW4NoteKeeperEx2.Infrastructure.Middleware;
using HW4NoteKeeperEx2.Infrastructure.Services;

namespace HW4NoteKeeper
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Configure
            RegistererConfigureSettings.AddConfiguration(builder);
            RegistererConfigureSettings.AddAiSettings(builder);

            // Services
            RegistererApplicationInsights.AddApplicationInsights(builder);  // Add early to include telemetry in all services.
            builder.Services.AddControllers();
            builder.Services.AddOpenApi();
            RegistererSwagger.AddSwagger(builder);
            RegistererEntityFramework.AddEntityFramework(builder);
            RegistererMyOpenAiClient.AddOpenAiClient(builder);              // Must be placed after AI settings have been registered.
            RegistererBlobStorageService.AddBlobStorageService(builder);
            RegistererZipRequestQueueService.AddZipQueueService(builder);

            // Development only
            if (builder.Environment.IsDevelopment())
            {
                builder.Logging.AddConsole();
            }

            // Build
            var app = builder.Build();

            // Validate services
            ValidateServices.Validate(app);

            // Seed database if needed
            await RegistererEntityFramework.InitalizeDatabase(app);

            //Add midelware, etc.
            RegistererSwagger.UseSwagger(app);
            app.MapOpenApi();
            app.UseMiddleware<MethodNotAllowedMiddleware>();                // Must be placed before MapControllers.
            app.UseHttpsRedirection();
            app.UseAuthorization();

            // Ensures controllers handle requests after middleware processing.
            app.MapControllers();

            // Start Application
            app.Run();
        }
    }
}
