using Azure.Storage.Blobs;
using Azure.Storage.Queues;
using HW4NoteKeeperWebJobTelemetry;
using HW4NoteKeeperWebJob.Models;
using Newtonsoft.Json;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using HW4NoteKeeperWebJob.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.ApplicationInsights.DependencyCollector;
using Microsoft.ApplicationInsights;
using HW4NoteKeeperWebJob.MessageProcessors;

/*
 This was heavily changed as I worked through the getting my function to work on Azure.
*/

var host = new HostBuilder()
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

        services.AddLogging(logging =>
        {
            logging.AddApplicationInsights();
            logging.SetMinimumLevel(LogLevel.Information);
        });

        services.Configure<TelemetryConfiguration>((config) =>
        {
            config.TelemetryInitializers.Add(new OperationCorrelationTelemetryInitializer());
            config.TelemetryInitializers.Add(new HttpDependenciesParsingTelemetryInitializer());
        });

        services.Configure<TelemetryConfiguration>((config) =>
        {
            config.TelemetryInitializers.Add(new OperationCorrelationTelemetryInitializer());
            config.TelemetryInitializers.Add(new HttpDependenciesParsingTelemetryInitializer());
        });

        services.AddSingleton<TelemetryClient>();

        services.AddSingleton<ITelemetry, Telemetry>();
        services.AddSingleton<IConfiguration>(configuration);
        services.AddSingleton(_ => new BlobServiceClient(azureWebJobsStorage));
        services.AddSingleton<IMessageProcessor, MessageProcessor>();
        services.AddSingleton(_ => new QueueClient(azureWebJobsStorage, "attachment-zip-requests-wj-ex2"));
    })
    .Build();

await RunWebJob(host.Services);

async Task RunWebJob(IServiceProvider services)
{
    var telemetry = services.GetRequiredService<ITelemetry>();
    var messageHandler = services.GetRequiredService<IMessageProcessor>();
    var queueClient = services.GetRequiredService<QueueClient>();
    var logger = services.GetRequiredService<ILoggerFactory>().CreateLogger("WebJob");

    logger.LogInformation("WebJob started. Polling queue...");

    while (true)
    {
        var response = await queueClient.ReceiveMessageAsync();

        if (response?.Value != null)
        {
            var rawMessage = response.Value.MessageText;
            telemetry.LogInformation($"Dequeued message: {rawMessage}");

            try
            {
                var message = JsonConvert.DeserializeObject<ZipQueueMessage>(rawMessage);
                ValidateQueueMessage(message);

                if (message == null)
                {
                    throw new Exception("This exception cannot be hit if validate queue message is working correctly.  It removes warning.");
                }

                await messageHandler.Process(message);
                await queueClient.DeleteMessageAsync(response.Value.MessageId, response.Value.PopReceipt);
            }
            catch (Exception ex)
            {
                telemetry.LogError("Error while processing queue message.", ex);
            }
        }
        else
        {
            await Task.Delay(5000); // wait before polling again
        }
    }
}

static void ValidateQueueMessage(ZipQueueMessage? message)
{
    if (message == null)
        throw new ArgumentNullException(nameof(message), "Queue message cannot be null.");

    if (message.NoteId == Guid.Empty)
        throw new ArgumentException("NoteId cannot be empty.");

    if (string.IsNullOrWhiteSpace(message.ZipFileId))
        throw new ArgumentException("ZipFileId cannot be null or empty.");
}
