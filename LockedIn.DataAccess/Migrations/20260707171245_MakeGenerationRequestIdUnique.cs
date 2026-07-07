using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LockedIn.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class MakeGenerationRequestIdUnique : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_addon_quota_reservations_generation_request_id",
                table: "addon_quota_reservations");

            migrationBuilder.CreateIndex(
                name: "ix_addon_quota_reservations_generation_request_id",
                table: "addon_quota_reservations",
                column: "generation_request_id",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_addon_quota_reservations_generation_request_id",
                table: "addon_quota_reservations");

            migrationBuilder.CreateIndex(
                name: "ix_addon_quota_reservations_generation_request_id",
                table: "addon_quota_reservations",
                column: "generation_request_id");
        }
    }
}
