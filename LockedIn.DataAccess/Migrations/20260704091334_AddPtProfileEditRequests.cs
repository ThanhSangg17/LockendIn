using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LockedIn.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddPtProfileEditRequests : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "pt_profile_edit_requests",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "(newid())"),
                    pt_profile_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    current_bio = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    current_specialization = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    current_experience_years = table.Column<int>(type: "int", nullable: false),
                    requested_bio = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    requested_specialization = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    requested_experience_years = table.Column<int>(type: "int", nullable: false),
                    status = table.Column<int>(type: "int", nullable: false, defaultValue: 1),
                    rejection_reason = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    requested_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(sysutcdatetime())"),
                    reviewed_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    reviewed_by_admin_id = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(sysutcdatetime())"),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_pt_profile_edit_requests", x => x.id);
                    table.ForeignKey(
                        name: "fk_pt_profile_edit_requests_admin_id",
                        column: x => x.reviewed_by_admin_id,
                        principalTable: "users",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_pt_profile_edit_requests_pt_profile_id",
                        column: x => x.pt_profile_id,
                        principalTable: "pt_profiles",
                        principalColumn: "id");
                });

            migrationBuilder.CreateIndex(
                name: "ix_pt_profile_edit_requests_pt_profile_id",
                table: "pt_profile_edit_requests",
                column: "pt_profile_id");

            migrationBuilder.CreateIndex(
                name: "IX_pt_profile_edit_requests_reviewed_by_admin_id",
                table: "pt_profile_edit_requests",
                column: "reviewed_by_admin_id");

            migrationBuilder.CreateIndex(
                name: "ix_pt_profile_edit_requests_status",
                table: "pt_profile_edit_requests",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "ux_pt_profile_edit_requests_one_pending",
                table: "pt_profile_edit_requests",
                column: "pt_profile_id",
                unique: true,
                filter: "([status]=(1))");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "pt_profile_edit_requests");
        }
    }
}
