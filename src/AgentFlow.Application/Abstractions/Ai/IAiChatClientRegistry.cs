namespace AgentFlow.Application.Abstractions.Ai;

public interface IAiChatClientRegistry
{
    IAiChatClient Resolve(string provider);
}
