using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LockedIn.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddDisputeHardening : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "original_booking_status",
                table: "disputes",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "original_settlement_status",
                table: "disputes",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "withdrawn_at",
                table: "disputes",
                type: "datetime2",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "ux_disputes_one_active_dispute_per_booking",
                table: "disputes",
                column: "booking_id",
                unique: true,
                filter: "([status] IN ((1), (2)))");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ux_disputes_one_active_dispute_per_booking",
                table: "disputes");

            migrationBuilder.DropColumn(
                name: "original_booking_status",
                table: "disputes");

            migrationBuilder.DropColumn(
                name: "original_settlement_status",
                table: "disputes");

            migrationBuilder.DropColumn(
                name: "withdrawn_at",
                table: "disputes");
        }
    }
}
