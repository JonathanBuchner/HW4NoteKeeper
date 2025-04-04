using HW4NoteKeeper.Infrastructure.Settings;
using Microsoft.ApplicationInsights.AspNetCore.Extensions;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.ApplicationInsights.Extensibility.PerfCounterCollector.QuickPulse;

namespace HW4NoteKeeper.Infrastructure.Services
{
    /// <summary>
    /// Adds application insights to project
    /// </summary>
    public static class RegistererApplicationInsights
    {
        /// <summary>
        /// Adds application insights to project.
        /// </summary>
        /// <param name="builder">builder</param>
        public static void AddApplicationInsights(WebApplicationBuilder builder)
        {
            var settings = builder.Configuration.GetSection(key: "ApplicationInsightsSettings").Get<ApplicationInsightsSettings>() ?? new ApplicationInsightsSettings();

            var options = new ApplicationInsightsServiceOptions()
            {
                EnableAdaptiveSampling = settings.EnableAdaptiveSampling,
                ConnectionString = settings.ConnectionString,
            };

            builder.Services.AddApplicationInsightsTelemetry(options);

            // Register custom TelemetryInitializer to provide role name when running locally
            builder.Services.AddSingleton<ITelemetryInitializer, DevelopmentRoleNameTelemetryInitializer>();

            builder.Services.ConfigureTelemetryModule<QuickPulseTelemetryModule>((module, _) =>
            {
                module.AuthenticationApiKey = settings.ApiKey;
            });

            // JB (2/25) Removed snapshop debug from the project.
        }
    }

    /// <summary>
    /// Custom telemetry initializer to set role name when running locally
    /// </summary>
    public class DevelopmentRoleNameTelemetryInitializer : ITelemetryInitializer
    {
        /// <summary>
        /// The web host environment
        /// </summary>
        private readonly IWebHostEnvironment _env;

        public DevelopmentRoleNameTelemetryInitializer(IWebHostEnvironment env)
        {
            _env = env;
        }

        /// <summary>
        /// Initializes the telemetry.
        /// </summary>
        /// <param name="telemetry">The telemetry to initialize.</param>
        public void Initialize(ITelemetry telemetry)
        {
            if (_env.IsDevelopment())
            {
                // Set the role name to the machine name if running in development environment
                telemetry.Context.Cloud.RoleName = Environment.MachineName;
            }
        }
    }
}


