namespace AgentFlow.Application.Abstractions.Identity;

public interface IJwtTokenGenerator
{
    AccessToken Generate(Guid userId, string email, string displayName);
}

public sealed record AccessToken(string Value, DateTimeOffset ExpiresAtUtc);
