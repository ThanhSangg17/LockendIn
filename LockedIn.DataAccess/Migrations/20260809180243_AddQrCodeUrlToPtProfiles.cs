using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LockedIn.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddQrCodeUrlToPtProfiles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "qr_code_url",
                table: "pt_profiles",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "qr_code_url",
                table: "pt_profiles");
        }
    }
}
