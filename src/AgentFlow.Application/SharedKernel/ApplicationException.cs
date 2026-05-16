namespace AgentFlow.Application.SharedKernel;

public abstract class ApplicationException : Exception
{
    protected ApplicationException(string message)
        : base(message)
    {
    }
}

public sealed class ValidationException : ApplicationException
{
    public ValidationException(string message)
        : base(message)
    {
    }
}

public sealed class NotFoundException : ApplicationException
{
    public NotFoundException(string message)
        : base(message)
    {
    }
}

public sealed class ConflictException : ApplicationException
{
    public ConflictException(string message)
        : base(message)
    {
    }
}

public sealed class AuthenticationException : ApplicationException
{
    public AuthenticationException(string message)
        : base(message)
    {
    }
}

public sealed class ForbiddenException : ApplicationException
{
    public ForbiddenException(string message)
        : base(message)
    {
    }
}
