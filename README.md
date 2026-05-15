# AgentFlow Platform

Production-oriented .NET 10 backend scaffold for a multi-tenant AI workflow automation platform.

The repository is intentionally focused on project organization and bootstrap infrastructure. Business logic for workflow orchestration, execution, auth flows, AI providers, and webhook processing should be added in later phases inside the existing boundaries.

## Solution Layout

```text
src/
  AgentFlow.Api/              ASP.NET Core HTTP API host
  AgentFlow.Worker/           Background worker host for distributed execution
  AgentFlow.Application/      Use cases, ports, interfaces, orchestration contracts
  AgentFlow.Domain/           Entities, aggregates, value objects, domain events
  AgentFlow.Infrastructure/   PostgreSQL, Redis, RabbitMQ, OpenTelemetry, security adapters
  AgentFlow.Contracts/        Shared commands, events, DTOs, message envelopes

tests/
  AgentFlow.UnitTests/
  AgentFlow.IntegrationTests/

deploy/
  otel/                        OpenTelemetry Collector configuration
```

## Architectural Rules

- Domain has no dependency on Application or Infrastructure.
- Application depends on Domain and Contracts.
- Infrastructure implements external adapters and depends inward on Application and Domain.
- API and Worker are composition roots.
- Contracts contains versioned integration messages shared by hosts and workers.
- PostgreSQL is the system of record.
- RabbitMQ is the asynchronous transport.
- Redis is for caching, coordination, idempotency, and rate-limit state.
- OpenTelemetry is the observability spine across API and worker processes.

## Local Bootstrap

```powershell
dotnet restore AgentFlow.slnx
dotnet build AgentFlow.slnx
docker compose up --build
```

Useful local URLs:

- API: http://localhost:8080/system/info
- RabbitMQ Management: http://localhost:15672
- OTLP gRPC: http://localhost:4317
- OTLP HTTP: http://localhost:4318

## Current Scope

Included:

- .NET 10 solution and project structure
- Clean Architecture dependency direction
- ASP.NET Core API host
- background worker host
- PostgreSQL EF Core bootstrap
- Redis distributed cache bootstrap
- RabbitMQ configuration/topology structure
- OpenTelemetry tracing/metrics bootstrap
- JSON structured console logging
- Dockerfiles and Docker Compose
- shared event/message contracts project

Not included yet:

- workflow CRUD
- execution engine
- real RabbitMQ publishers/consumers
- JWT issuing endpoints
- tenant model
- provider-specific AI integrations
- migrations
