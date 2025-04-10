using Azure.Storage.Blobs;
using HW4AzureFunctions.Interfaces;
using HW4AzureFunctions.MessageProcessors;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

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
        var azureWebJobsStorage = context.Configuration["AzureWebJobsStorage"];

        if (string.IsNullOrWhiteSpace(azureWebJobsStorage))
        {
            throw new InvalidOperationException("Must configure 'AzureWebJobsStorage'.");
        }

        services.AddSingleton(x => new BlobServiceClient(azureWebJobsStorage));
        services.AddSingleton<IMessageProcessor, MessageProcessor>();
    })
    .Build();

host.Run();