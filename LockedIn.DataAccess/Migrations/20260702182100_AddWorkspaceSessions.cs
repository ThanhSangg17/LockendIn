using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LockedIn.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddWorkspaceSessions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "workspace_sessions",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "(newid())"),
                    workspace_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    session_number = table.Column<int>(type: "int", nullable: false),
                    description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    completed_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(sysutcdatetime())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__workspace_sessions__3213E83FD1C72652", x => x.id);
                    table.ForeignKey(
                        name: "fk_workspace_sessions_workspace_id",
                        column: x => x.workspace_id,
                        principalTable: "workspaces",
                        principalColumn: "id");
                });

            migrationBuilder.CreateIndex(
                name: "uq_workspace_sessions_workspace_id_session_number",
                table: "workspace_sessions",
                columns: new[] { "workspace_id", "session_number" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "workspace_sessions");
        }
    }
}
