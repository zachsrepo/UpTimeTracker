using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UpTimeTracker.Migrations
{
    /// <inheritdoc />
    public partial class checktype : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Type",
                table: "MonitoredServices",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Type",
                table: "MonitoredServices");
        }
    }
}
