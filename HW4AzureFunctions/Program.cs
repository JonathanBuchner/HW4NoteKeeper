using Azure.Storage.Blobs;
using Azure.Identity; // Fix for CS0246: Add the missing namespace for DefaultAzureCredential
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using HW4AzureFunctions.Interfaces;
using HW4AzureFunctions.MessageProcessors;

var builder = FunctionsApplication.CreateBuilder(args);

builder.ConfigureFunctionsWebApplication();

builder.Configuration
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
    .AddUserSecrets<Program>(optional: true);

builder.ConfigureFunctionsWebApplication();

var blobServiceUri = builder.Configuration["StorageConnections:blobServiceUri"];
var azureWebJobsStorage = builder.Configuration["AzureWebJobsStorage"];

// Set up blob service client.
if (!string.IsNullOrWhiteSpace(azureWebJobsStorage))
{
    throw new InvalidOperationException("Must configure either 'AzureWebJobsStorage'.");
}

builder.Services.AddSingleton(x => new BlobServiceClient(azureWebJobsStorage));
builder.Services.AddSingleton<IMessageProcessor, MessageProcessor>();

builder.Build().Run();
