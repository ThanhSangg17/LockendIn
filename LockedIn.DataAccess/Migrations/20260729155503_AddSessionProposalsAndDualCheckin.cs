using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LockedIn.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddSessionProposalsAndDualCheckin : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "completed_at",
                table: "workspace_sessions",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AddColumn<DateTime>(
                name: "customer_checked_in_at",
                table: "workspace_sessions",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "pt_checked_in_at",
                table: "workspace_sessions",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "scheduled_end",
                table: "workspace_sessions",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "scheduled_start",
                table: "workspace_sessions",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "started_at",
                table: "workspace_sessions",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "status",
                table: "workspace_sessions",
                type: "int",
                nullable: false,
                defaultValue: 3);

            migrationBuilder.CreateTable(
                name: "session_proposals",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "(newid())"),
                    workspace_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    session_number = table.Column<int>(type: "int", nullable: false),
                    status = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(sysutcdatetime())"),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_session_proposals", x => x.id);
                    table.ForeignKey(
                        name: "fk_session_proposals_workspace_id",
                        column: x => x.workspace_id,
                        principalTable: "workspaces",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "session_proposal_slots",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "(newid())"),
                    proposal_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    slot_code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    is_selected = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_session_proposal_slots", x => x.id);
                    table.ForeignKey(
                        name: "fk_session_proposal_slots_proposal_id",
                        column: x => x.proposal_id,
                        principalTable: "session_proposals",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_session_proposal_slots_proposal_id",
                table: "session_proposal_slots",
                column: "proposal_id");

            migrationBuilder.CreateIndex(
                name: "ix_session_proposals_workspace_session_status",
                table: "session_proposals",
                columns: new[] { "workspace_id", "session_number", "status" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "session_proposal_slots");

            migrationBuilder.DropTable(
                name: "session_proposals");

            migrationBuilder.DropColumn(
                name: "customer_checked_in_at",
                table: "workspace_sessions");

            migrationBuilder.DropColumn(
                name: "pt_checked_in_at",
                table: "workspace_sessions");

            migrationBuilder.DropColumn(
                name: "scheduled_end",
                table: "workspace_sessions");

            migrationBuilder.DropColumn(
                name: "scheduled_start",
                table: "workspace_sessions");

            migrationBuilder.DropColumn(
                name: "started_at",
                table: "workspace_sessions");

            migrationBuilder.DropColumn(
                name: "status",
                table: "workspace_sessions");

            migrationBuilder.AlterColumn<DateTime>(
                name: "completed_at",
                table: "workspace_sessions",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);
        }
    }
}
