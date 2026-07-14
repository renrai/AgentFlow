namespace AgentFlow.Infrastructure.AiProviders;

public sealed class AiProviderOptions
{
    public const string SectionName = "AiProviders";

    public AnthropicOptions Anthropic { get; init; } = new();

    public OpenAiOptions OpenAi { get; init; } = new();

    public sealed class AnthropicOptions
    {
        public string BaseUrl { get; init; } = "https://api.anthropic.com";

        public string ApiKey { get; init; } = string.Empty;

        public string ApiVersion { get; init; } = "2023-06-01";
    }

    public sealed class OpenAiOptions
    {
        public string BaseUrl { get; init; } = "https://api.openai.com";

        public string ApiKey { get; init; } = string.Empty;
    }
}
