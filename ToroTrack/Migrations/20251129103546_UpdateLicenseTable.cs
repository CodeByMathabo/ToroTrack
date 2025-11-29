using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ToroTrack.Migrations
{
    /// <inheritdoc />
    public partial class UpdateLicenseTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "Cost",
                table: "Licenses",
                type: "numeric(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "Source",
                table: "Licenses",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Type",
                table: "Licenses",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Cost",
                table: "Licenses");

            migrationBuilder.DropColumn(
                name: "Source",
                table: "Licenses");

            migrationBuilder.DropColumn(
                name: "Type",
                table: "Licenses");
        }
    }
}
