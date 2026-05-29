using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AgentFlow.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Phase3ExecutionEngine : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "workflow_executions",
                schema: "workflow",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    workflow_id = table.Column<Guid>(type: "uuid", nullable: false),
                    workflow_version = table.Column<int>(type: "integer", nullable: false),
                    status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    trigger_payload = table.Column<string>(type: "jsonb", nullable: true),
                    error_message = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    created_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    started_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    completed_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_workflow_executions", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "execution_steps",
                schema: "workflow",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    workflow_execution_id = table.Column<Guid>(type: "uuid", nullable: false),
                    node_id = table.Column<Guid>(type: "uuid", nullable: false),
                    node_type = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    node_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    sequence = table.Column<int>(type: "integer", nullable: false),
                    status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    input = table.Column<string>(type: "jsonb", nullable: true),
                    output = table.Column<string>(type: "jsonb", nullable: true),
                    error_message = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    created_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    started_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    completed_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_execution_steps", x => x.id);
                    table.ForeignKey(
                        name: "FK_execution_steps_workflow_executions_workflow_execution_id",
                        column: x => x.workflow_execution_id,
                        principalSchema: "workflow",
                        principalTable: "workflow_executions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_execution_steps_workflow_execution_id",
                schema: "workflow",
                table: "execution_steps",
                column: "workflow_execution_id");

            migrationBuilder.CreateIndex(
                name: "IX_workflow_executions_status",
                schema: "workflow",
                table: "workflow_executions",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "IX_workflow_executions_tenant_id_workflow_id",
                schema: "workflow",
                table: "workflow_executions",
                columns: new[] { "tenant_id", "workflow_id" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "execution_steps",
                schema: "workflow");

            migrationBuilder.DropTable(
                name: "workflow_executions",
                schema: "workflow");
        }
    }
}
