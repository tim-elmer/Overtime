using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Overtime.Migrations
{
    /// <inheritdoc />
    public partial class NoRemind : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "NoRemind",
                table: "UserTimeZones",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NoRemind",
                table: "UserTimeZones");
        }
    }
}
