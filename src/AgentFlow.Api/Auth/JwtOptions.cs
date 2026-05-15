namespace AgentFlow.Api.Auth;

public sealed class JwtOptions
{
    public const string SectionName = "Authentication:Jwt";

    public string Issuer { get; init; } = "AgentFlow";

    public string Audience { get; init; } = "AgentFlow";

    public string SigningKey { get; init; } = string.Empty;

    public int ExpirationMinutes { get; init; } = 60;
}
