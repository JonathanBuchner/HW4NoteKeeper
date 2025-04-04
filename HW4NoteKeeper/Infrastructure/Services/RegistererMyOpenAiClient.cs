using Azure.AI.OpenAI;
using Azure;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Options;
using HW4NoteKeeper.Infrastructure.Settings;

namespace HW4NoteKeeper.Infrastructure.Services
{
    /// <summary>
    /// A static class to register the OpenAI client with the dependency injection container.
    /// </summary>
    public static class RegistererMyOpenAiClient
    {
        /// <summary>
        /// Registers the OpenAI client with the dependency injection container.
        /// </summary>
        /// <param name="builder">web applicatoin builder</param>
        public static void AddOpenAiClient(WebApplicationBuilder builder)
        {
            var settings = builder.Services.BuildServiceProvider().GetRequiredService<AiModelSettings>();

            var openAiServiceEndpoint = new Uri(settings.EndpointUri);
            var apiKeyCredential = new AzureKeyCredential(settings.ApiKey);
            var deploymentModelName = settings.DeploymentModelName;

            builder.Services.AddChatClient(services =>
                new AzureOpenAIClient(openAiServiceEndpoint, apiKeyCredential)
                    .GetChatClient(deploymentModelName)
                    .AsChatClient());

            builder.Services.AddScoped<MyOpenAiClient>(); // Needed for dependency injection
        }
    }
}
