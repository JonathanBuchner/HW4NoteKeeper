namespace HW4NoteKeeper.Infrastructure.Settings
{
    public class AiModelSettings
    {
        public required string EndpointUri { get; set; }
        public required string ApiKey { get; set; }
        public string DeploymentModelName { get; set; } = "gpt-4o-mini";
        public int MaxOutputTokens { get; set; } = 500;
    }
}
