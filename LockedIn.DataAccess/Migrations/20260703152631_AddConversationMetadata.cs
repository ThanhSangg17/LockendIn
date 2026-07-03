using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LockedIn.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddConversationMetadata : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "last_activity_at",
                table: "conversations",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "last_message_preview",
                table: "conversations",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "last_activity_at",
                table: "conversations");

            migrationBuilder.DropColumn(
                name: "last_message_preview",
                table: "conversations");
        }
    }
}
