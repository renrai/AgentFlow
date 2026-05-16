using AgentFlow.Domain.SharedKernel;
using AgentFlow.Domain.Users;

namespace AgentFlow.UnitTests.Users;

public sealed class UserTests
{
    [Fact]
    public void CreateShouldNormalizeEmailAndActivateUser()
    {
        var now = DateTimeOffset.UtcNow;

        var user = User.Create(" PERSON@Example.COM ", "Person One", "hashed-password", now);

        Assert.NotEqual(Guid.Empty, user.Id);
        Assert.Equal("person@example.com", user.Email);
        Assert.Equal("Person One", user.DisplayName);
        Assert.Equal(UserStatus.Active, user.Status);
        Assert.Equal(now, user.CreatedAtUtc);
        Assert.True(user.IsActive);
    }

    [Fact]
    public void CreateShouldRejectInvalidEmail()
    {
        var exception = Assert.Throws<DomainException>(() =>
            User.Create("not-an-email", "Person One", "hashed-password", DateTimeOffset.UtcNow));

        Assert.Contains("valid email", exception.Message, StringComparison.OrdinalIgnoreCase);
    }
}
