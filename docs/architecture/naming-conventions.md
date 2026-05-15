# Naming Conventions

## Projects

Project names use the `AgentFlow.<Layer>` pattern:

- `AgentFlow.Api`
- `AgentFlow.Worker`
- `AgentFlow.Application`
- `AgentFlow.Domain`
- `AgentFlow.Infrastructure`
- `AgentFlow.Contracts`

## Messages

Integration messages are versioned with a `V<number>` suffix:

- `WorkflowExecutionRequestedV1`
- `WorkflowStepScheduledV1`
- `WorkflowExecutionCompletedV1`

Routing keys use lower-case dotted names:

- `workflow.execution.requested`
- `workflow.step.scheduled`
- `workflow.dead-letter`

## Database

Tables should use snake case:

- `tenants`
- `workflow_versions`
- `workflow_executions`
- `workflow_step_executions`
- `outbox_messages`
- `inbox_messages`

Tenant-owned tables should include:

- `tenant_id`
- `created_at`
- `updated_at`

## API

REST routes should use plural resources:

- `/workflows`
- `/workflows/{workflowId}/executions`
- `/executions/{executionId}`
- `/webhooks/{triggerId}`

System endpoints live under:

- `/system/*`
- `/health/*`

## Configuration

Configuration sections should match adapter names:

- `Authentication:Jwt`
- `ConnectionStrings:PostgreSql`
- `ConnectionStrings:Redis`
- `RabbitMq`
- `OpenTelemetry`
