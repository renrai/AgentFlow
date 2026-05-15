# Monorepo Organization

## Projects

| Project | Responsibility |
|---|---|
| `AgentFlow.Api` | HTTP boundary, authentication middleware, request routing, OpenAPI, health endpoints |
| `AgentFlow.Worker` | background host for queue consumers, workflow execution, retry handling |
| `AgentFlow.Application` | use cases, orchestration services, ports, application-level policies |
| `AgentFlow.Domain` | aggregates, entities, value objects, domain events, invariants |
| `AgentFlow.Infrastructure` | adapters for PostgreSQL, Redis, RabbitMQ, OpenTelemetry, external providers |
| `AgentFlow.Contracts` | versioned commands, integration events, DTOs, message envelopes |

## Dependency Direction

```text
Api --------\
Worker -----+--> Application --> Domain
            |          |
            |          v
            +--> Infrastructure --> Domain

Application --> Contracts
Infrastructure --> Contracts
Api/Worker --> Contracts
```

Infrastructure may reference Application because it implements application ports. Application must not reference Infrastructure.

## Feature Placement

- New domain behavior goes under `AgentFlow.Domain/<Context>`.
- New commands/queries/use cases go under `AgentFlow.Application/<Context>`.
- Database mappings and external SDK adapters go under `AgentFlow.Infrastructure/<Adapter>`.
- HTTP endpoints/controllers go under `AgentFlow.Api/<Feature>`.
- Worker consumers go under `AgentFlow.Worker/Consumers`.
- Cross-process events go under `AgentFlow.Contracts/Events`.

## Bounded Context Folders

The initial folders mirror the platform roadmap:

- `Auth`
- `Tenants`
- `Workflows`
- `Executions`
- `Webhooks`
- `Integrations`
- `AiProviders`
- `Observability`
- `Messaging`
- `Persistence`
