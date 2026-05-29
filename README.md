# AgentFlow Platform

Production-grade .NET 10 backend for a multi-tenant AI workflow automation platform — think Zapier or n8n, but built around Clean Architecture, DDD, CQRS, and a real distributed execution engine on top of PostgreSQL + RabbitMQ.

This repository is a portfolio project demonstrating senior-level backend engineering practices: bounded contexts, aggregate-driven domain logic, async messaging with dead-letter handling, schema-isolated multi-tenancy, structured observability, and a workflow execution graph with topological scheduling.

## Status

| Phase | Scope | State |
|---|---|---|
| 0 | Solution scaffold, Docker Compose, OpenTelemetry, structured logging | Done |
| 1 | Identity, JWT auth, password hashing, multi-tenant model | Done |
| 2 | Workflow CRUD, versioning, publish / archive lifecycle | Done |
| 3 | Execution engine, RabbitMQ publisher / consumer, node executor strategy, end-to-end async runs | Done |
| 4 | Webhook triggers, execution history filters, step retry view | Planned |
| 5 | AI provider integrations (OpenAI, Anthropic), prompt nodes | Planned |
| 6 | Retry policies, DLQ replay, distributed tracing across hops | Planned |
| 7 | CI/CD, deployment polish, performance benchmarks | Planned |

## Architecture

```
+----------------------+         +-------------------------+
|     AgentFlow.Api    |  HTTP   |   AgentFlow.Worker      |
|  (ASP.NET Core 10)   |         |   (BackgroundService)   |
|                      |         |                         |
|  Auth / Workflows /  |         |  Consumer + Executor +  |
|  Executions endpoints|         |  Node Executor Registry |
+----------+-----------+         +-----------+-------------+
           |                                 |
           |  publish WorkflowExecutionRequestedV1
           |                                 |
           v                                 |
   +-------+---------------------------------+-------+
   |                  RabbitMQ                       |
   |  topic exchange + execution queue + DLX/DLQ     |
   +-------------------------------------------------+
           ^                                 ^
           |                                 |
+----------+---------------------------------+-----------+
|              AgentFlow.Infrastructure                  |
|  Persistence (EF Core 10 + Npgsql)                     |
|  Messaging   (RabbitMQ.Client 7.x async API)           |
|  Identity    (JsonWebTokenHandler + PBKDF2-SHA256)     |
|  Observability (OpenTelemetry traces / metrics / logs) |
+--------------------------------------------------------+
           |                                 |
           v                                 v
   +-------+------+                  +-------+------+
   | PostgreSQL   |                  |    Redis     |
   | identity +   |                  |   caching    |
   | workflow     |                  |              |
   | schemas      |                  |              |
   +--------------+                  +--------------+
```

Layering follows Clean Architecture strictly:

```
Domain <- Application <- Infrastructure <- (API, Worker)
                    |
                    +-- Contracts (shared messaging records)
```

- **Domain** has zero external dependencies. Pure C#.
- **Application** owns CQRS handlers and ports (interfaces). No persistence or HTTP knowledge.
- **Infrastructure** implements every port: EF Core, RabbitMQ, JWT, OTel.
- **API** and **Worker** are independent composition roots.
- **Contracts** holds versioned `IIntegrationEvent` records shared across hosts.

## What Each Phase Delivers

### Phase 1 — Identity & Tenancy

- `User` aggregate with email normalization and PBKDF2-SHA256 password hashing (100k iterations, `CryptographicOperations.FixedTimeEquals`).
- `Tenant` aggregate that auto-creates an Owner `TenantMember` on construction.
- JWT issuance via modern `JsonWebTokenHandler` (not the deprecated `JwtSecurityTokenHandler`).
- All authenticated endpoints check `ITenantContext` + membership on every request.
- Schema-isolated multi-tenancy: `identity.users`, `identity.tenants`, `identity.tenant_members` in PostgreSQL.
- Endpoints: `POST /auth/register`, `POST /auth/login`, `GET /me`, `POST /tenants`, `GET /tenants/me`.

### Phase 2 — Workflow Definition

- `Workflow` aggregate with `WorkflowNode` and `WorkflowEdge` child entities.
- Lifecycle state machine: `Draft` → `Published` → `Archived`. Re-publishing bumps `Version`.
- Edge integrity enforced inside the aggregate (no orphan edges).
- JSONB columns for node configuration.
- Endpoints: `POST /tenants/{tenantId}/workflows`, `GET`, `PUT`, `POST /publish`, `POST /archive`.

### Phase 3 — Execution Engine

- `WorkflowExecution` aggregate with `ExecutionStep` children, both with state-machine invariants.
- `StartExecution` handler creates the execution in `Pending`, then publishes a `WorkflowExecutionRequestedV1` integration event.
- `RabbitMqMessagePublisher` (singleton) serializes to JSON and publishes to a topic exchange.
- `RabbitMqTopologyInitializer` (hosted service) declares the exchange, queue, dead-letter exchange, and DLQ on startup.
- `WorkflowExecutionConsumer` (BackgroundService) with `prefetch=1`, per-message DI scope, ack/nack semantics.
- `WorkflowExecutor` performs topological sort (Kahn's algorithm) of the workflow graph, dispatches each node through `INodeExecutorRegistry`, persists step input/output as JSONB, and aborts on first failure.
- Built-in node executors: `noop` (fallback) and `action.http` (real HTTP calls with header propagation).
- Endpoints: `POST /tenants/{tenantId}/workflows/{workflowId}/executions`, `GET /tenants/{tenantId}/executions/{executionId}`, `GET /tenants/{tenantId}/workflows/{workflowId}/executions`.

## Tech Stack

| Concern | Choice |
|---|---|
| Runtime | .NET 10, C# latest |
| Web | ASP.NET Core Minimal APIs |
| Data | EF Core 10, Npgsql, PostgreSQL 17 |
| Messaging | RabbitMQ 4 (async client 7.x) |
| Cache | Redis 7.4 |
| Auth | `Microsoft.IdentityModel.JsonWebTokens` 8.x |
| Observability | OpenTelemetry 1.15 (OTLP exporter) |
| Logging | `LoggerMessage.Define` source-gen style (CA1848-clean) |
| Container | Docker Compose for full local stack |
| Package management | Central Package Management via `Directory.Packages.props` |

## Local Setup

### Prerequisites

- .NET 10 SDK
- Docker Desktop
- A REST client (curl, Bruno, Postman, etc.)

### 1. Start infrastructure

```powershell
docker compose up -d postgres redis rabbitmq otel-collector
```

### 2. Run the API and Worker (two terminals)

```powershell
# Terminal A
dotnet run --project src/AgentFlow.Api

# Terminal B
dotnet run --project src/AgentFlow.Worker
```

The API auto-applies EF Core migrations on startup in `Development`.

### 3. Useful local URLs

| Service | URL |
|---|---|
| API system info | http://localhost:8080/system/info |
| API health | http://localhost:8080/health/live |
| OpenAPI document | http://localhost:8080/openapi/v1.json |
| RabbitMQ management | http://localhost:15672 (`agentflow` / `agentflow`) |
| OTLP gRPC | http://localhost:4317 |

## End-to-End Example

The full flow exercises every component: HTTP, JWT, EF Core write, RabbitMQ publish, RabbitMQ consume, graph execution, external HTTP call, and persisted step output.

```powershell
# 1. Register an account (also creates an initial tenant)
curl -X POST http://localhost:8080/auth/register `
  -H "Content-Type: application/json" `
  -d '{"email":"demo@agentflow.dev","password":"Secret@123","displayName":"Demo","tenantName":"Demo Org"}'

# 2. Login
$loginResponse = curl -X POST http://localhost:8080/auth/login `
  -H "Content-Type: application/json" `
  -d '{"email":"demo@agentflow.dev","password":"Secret@123"}'
# Capture accessToken and tenantId for the next steps.

# 3. Create a workflow with a noop node and an HTTP node
curl -X POST http://localhost:8080/tenants/{tenantId}/workflows `
  -H "Authorization: Bearer {token}" `
  -H "Content-Type: application/json" `
  -d '{
    "name": "Hello httpbin",
    "description": "Posts the trigger payload to httpbin and echoes it back",
    "nodes": [
      {"type": "noop",        "name": "Start",        "positionX": 0,   "positionY": 0, "configuration": "{}"},
      {"type": "action.http", "name": "POST httpbin", "positionX": 300, "positionY": 0, "configuration": "{\"url\":\"https://httpbin.org/post\",\"method\":\"POST\"}"}
    ],
    "edges": []
  }'

# 4. Publish (Draft -> Published, version becomes 1)
curl -X POST http://localhost:8080/tenants/{tenantId}/workflows/{workflowId}/publish `
  -H "Authorization: Bearer {token}"

# 5. Trigger an execution
curl -X POST http://localhost:8080/tenants/{tenantId}/workflows/{workflowId}/executions `
  -H "Authorization: Bearer {token}" `
  -H "Content-Type: application/json" `
  -d '{"triggerPayload":"{\"message\":\"hello\"}"}'

# 6. Fetch the execution with its steps
curl http://localhost:8080/tenants/{tenantId}/executions/{executionId} `
  -H "Authorization: Bearer {token}"
```

Sample response (trimmed):

```json
{
  "executionId": "f5667156-1854-4481-af6d-0bc76e3ab649",
  "workflowId":  "88d9cdbf-fa56-4ef7-866a-be2b02f11f9c",
  "workflowVersion": 2,
  "status": "Completed",
  "triggerPayload": "{\"message\":\"hello\"}",
  "startedAtUtc":   "2026-05-29T15:16:50.936975+00:00",
  "completedAtUtc": "2026-05-29T15:16:52.837572+00:00",
  "steps": [
    {
      "nodeName": "POST httpbin",
      "nodeType": "action.http",
      "status":   "Completed",
      "input":    "{\"message\":\"hello\"}",
      "output":   "{\"body\":{\"json\":{\"message\":\"hello\"},\"url\":\"https://httpbin.org/post\",...},\"statusCode\":200}"
    },
    {
      "nodeName": "Start",
      "nodeType": "noop",
      "status":   "Completed",
      "input":    "{\"message\":\"hello\"}",
      "output":   "{\"message\":\"hello\"}"
    }
  ]
}
```

The OpenTelemetry `traceparent` header is propagated to the external HTTP call automatically.

## Solution Layout

```
src/
  AgentFlow.Api/              ASP.NET Core host. Endpoints, auth wiring, exception middleware.
  AgentFlow.Worker/           BackgroundService host. RabbitMQ consumer + execution engine.
  AgentFlow.Application/      Use cases (CQRS handlers), ports, application exceptions.
  AgentFlow.Domain/           Aggregates, entities, enums, guards, domain exceptions.
  AgentFlow.Infrastructure/   EF Core, RabbitMQ, JWT, password hashing, OTel.
  AgentFlow.Contracts/        Versioned integration events shared by hosts and workers.

tests/
  AgentFlow.UnitTests/
  AgentFlow.IntegrationTests/

deploy/
  otel/                        OpenTelemetry Collector configuration.
```

## Design Decisions Worth Calling Out

- **Exception-based domain validation.** Aggregates throw `DomainException`; application handlers throw a typed `ApplicationException` hierarchy (`ValidationException`, `NotFoundException`, `ConflictException`, `ForbiddenException`, `AuthenticationException`). The API exception middleware maps each to RFC 7807 ProblemDetails responses.
- **No MediatR.** CQRS handlers are plain sealed classes with `HandleAsync` methods. Less indirection, no reflection cost, IDE jump-to-definition works.
- **Synchronous in-process execution per message (Phase 3).** The entire workflow graph runs to completion inside one consumer scope. Distributed step-by-step scheduling with retries is a deliberate Phase 6 concern, not premature complexity.
- **Topological sort over graph traversal.** Allows future fan-in / fan-out without rewriting the executor; the merge-inputs logic already constructs a JSON object keyed by source node IDs.
- **Schema separation in PostgreSQL.** `identity.*` tables for auth/tenancy, `workflow.*` tables for workflows and executions. Future bounded contexts get their own schema.
- **Source-generated logging.** Every `ILogger` call site uses `LoggerMessage.Define` for zero-allocation logging (CA1848-clean across the whole solution).
- **Identity isolated from Worker.** `AddInfrastructure` deliberately excludes JWT registration; only the API calls `AddPlatformIdentity`. The Worker has no business knowing the JWT signing key.

## Roadmap

- **Phase 4 — Webhooks & History.** Catch-all `POST /webhooks/{tenantSlug}/{workflowSlug}` endpoint that starts a workflow execution from the request body. Execution history filters (status, date range, pagination). Optional HMAC-SHA256 signature verification.
- **Phase 5 — AI Provider Integration.** `INodeExecutor` implementations for OpenAI Chat Completions and Anthropic Messages, with provider abstraction and prompt-template nodes.
- **Phase 6 — Reliability & Observability.** Polly-based retry policies on node executors, DLQ replay tooling, distributed traces spanning API publish → Worker consume → external HTTP, metrics for execution counts / step latency / failure rate.
- **Phase 7 — Deployment Polish.** GitHub Actions CI (build, test, container publish), README with architecture diagram, K6 load test results, container image hardening.

## License

MIT
