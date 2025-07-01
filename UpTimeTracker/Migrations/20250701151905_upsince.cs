using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UpTimeTracker.Migrations
{
    /// <inheritdoc />
    public partial class upsince : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "UpSince",
                table: "MonitoredServices",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UpSince",
                table: "MonitoredServices");
        }
    }
}
