using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable
#pragma warning disable CA1707

namespace AgentFlow.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Phase4Webhooks : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1. Add the column nullable so existing rows aren't rejected.
            migrationBuilder.AddColumn<string>(
                name: "webhook_token",
                schema: "workflow",
                table: "workflows",
                type: "character varying(80)",
                maxLength: 80,
                nullable: true);

            // 2. Backfill existing workflows with a deterministic, unique token
            //    derived from their primary key.
            migrationBuilder.Sql(
                "UPDATE workflow.workflows " +
                "SET webhook_token = 'wh_' || lower(replace(id::text, '-', '')) " +
                "WHERE webhook_token IS NULL;");

            // 3. Enforce NOT NULL now that every row has a value.
            migrationBuilder.AlterColumn<string>(
                name: "webhook_token",
                schema: "workflow",
                table: "workflows",
                type: "character varying(80)",
                maxLength: 80,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(80)",
                oldMaxLength: 80,
                oldNullable: true);

            // 4. Unique index for fast token lookups.
            migrationBuilder.CreateIndex(
                name: "IX_workflows_webhook_token",
                schema: "workflow",
                table: "workflows",
                column: "webhook_token",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_workflows_webhook_token",
                schema: "workflow",
                table: "workflows");

            migrationBuilder.DropColumn(
                name: "webhook_token",
                schema: "workflow",
                table: "workflows");
        }
    }
}
