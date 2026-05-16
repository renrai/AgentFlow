using AgentFlow.Application.Abstractions.Clock;
using AgentFlow.Application.Abstractions.Identity;
using AgentFlow.Application.Abstractions.Persistence;
using AgentFlow.Application.SharedKernel;
using AgentFlow.Domain.Users;

namespace AgentFlow.Application.Identity.Login;

public sealed class LoginHandler(
    IUserRepository users,
    IPasswordHasher passwordHasher,
    IJwtTokenGenerator tokenGenerator,
    IUnitOfWork unitOfWork,
    IClock clock)
{
    private const string InvalidCredentials = "invalid email or password.";

    public async Task<LoginResult> HandleAsync(LoginCommand command, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        if (string.IsNullOrWhiteSpace(command.Email) || string.IsNullOrEmpty(command.Password))
        {
            throw new AuthenticationException(InvalidCredentials);
        }

        var user = await users.GetByEmailAsync(command.Email, cancellationToken).ConfigureAwait(false);
        if (user is null || user.Status != UserStatus.Active)
        {
            throw new AuthenticationException(InvalidCredentials);
        }

        if (!passwordHasher.Verify(command.Password, user.PasswordHash))
        {
            throw new AuthenticationException(InvalidCredentials);
        }

        var now = clock.UtcNow;
        user.MarkLogin(now);

        var token = tokenGenerator.Generate(user.Id, user.Email, user.DisplayName);

        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return new LoginResult(token.Value, token.ExpiresAtUtc, user.Id, user.Email, user.DisplayName);
    }
}
