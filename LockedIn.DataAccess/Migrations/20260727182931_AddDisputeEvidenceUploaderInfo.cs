using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LockedIn.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddDisputeEvidenceUploaderInfo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "uploaded_by_role",
                table: "dispute_evidences",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "uploaded_by_user_id",
                table: "dispute_evidences",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_dispute_evidences_uploaded_by_user_id",
                table: "dispute_evidences",
                column: "uploaded_by_user_id");

            migrationBuilder.AddForeignKey(
                name: "fk_dispute_evidences_uploaded_by_user_id",
                table: "dispute_evidences",
                column: "uploaded_by_user_id",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_dispute_evidences_uploaded_by_user_id",
                table: "dispute_evidences");

            migrationBuilder.DropIndex(
                name: "IX_dispute_evidences_uploaded_by_user_id",
                table: "dispute_evidences");

            migrationBuilder.DropColumn(
                name: "uploaded_by_role",
                table: "dispute_evidences");

            migrationBuilder.DropColumn(
                name: "uploaded_by_user_id",
                table: "dispute_evidences");
        }
    }
}
