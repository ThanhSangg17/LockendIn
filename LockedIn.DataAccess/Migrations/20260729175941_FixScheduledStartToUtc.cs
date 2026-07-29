using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LockedIn.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class FixScheduledStartToUtc : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                UPDATE workspace_sessions 
                SET scheduled_start = DATEADD(hour, -7, scheduled_start),
                    scheduled_end = DATEADD(hour, -7, scheduled_end)
                WHERE scheduled_start IS NOT NULL;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                UPDATE workspace_sessions 
                SET scheduled_start = DATEADD(hour, 7, scheduled_start),
                    scheduled_end = DATEADD(hour, 7, scheduled_end)
                WHERE scheduled_start IS NOT NULL;
            ");
        }

    }
}
