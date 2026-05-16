namespace AgentFlow.Application.Identity.Login;

public sealed record LoginCommand(string Email, string Password);

public sealed record LoginResult(
    string AccessToken,
    DateTimeOffset ExpiresAtUtc,
    Guid UserId,
    string Email,
    string DisplayName);
