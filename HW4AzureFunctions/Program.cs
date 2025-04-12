using Azure.Storage.Blobs;
using HW4AzureFunctions.Interfaces;
using HW4AzureFunctions.MessageProcessors;
using HW4AzureFunctions.Telemetry;
using Microsoft.ApplicationInsights.DependencyCollector;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

/*
 This was heavily changed as I worked through the getting my function to work on Azure.
*/

var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults()
    .ConfigureAppConfiguration((context, configBuilder) =>
    {
        configBuilder
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
            .AddUserSecrets<Program>(optional: true)
            .AddEnvironmentVariables();
    })
    .ConfigureServices((context, services) =>
    {
        var configuration = context.Configuration;
        var azureWebJobsStorage = configuration["AzureWebJobsStorage"];

        if (string.IsNullOrWhiteSpace(azureWebJobsStorage))
        {
            throw new InvalidOperationException("Must configure 'AzureWebJobsStorage'.");
        }

        services.AddApplicationInsightsTelemetryWorkerService();

        services.Configure<TelemetryConfiguration>((config) =>
        {
            config.TelemetryInitializers.Add(new OperationCorrelationTelemetryInitializer());
            config.TelemetryInitializers.Add(new HttpDependenciesParsingTelemetryInitializer());
        });

        services.AddSingleton<ITelemetry, Telemetry>();
        services.AddSingleton<IConfiguration>(configuration);
        services.AddSingleton(_ => new BlobServiceClient(azureWebJobsStorage));
        services.AddSingleton<IMessageProcessor, MessageProcessor>();
    })
    .Build();

host.Run();
