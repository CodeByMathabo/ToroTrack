using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ToroTrack.Migrations
{
    /// <inheritdoc />
    public partial class AddDeliveryDetailsToOrder : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "CatalogItemId",
                table: "ClientAssets",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "ClientAssets",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ImageUrl",
                table: "ClientAssets",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "ClientAssets",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "ClientAssets",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ContactNumber",
                table: "AssetOrders",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "DeliveryAddress",
                table: "AssetOrders",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Description",
                table: "ClientAssets");

            migrationBuilder.DropColumn(
                name: "ImageUrl",
                table: "ClientAssets");

            migrationBuilder.DropColumn(
                name: "Name",
                table: "ClientAssets");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "ClientAssets");

            migrationBuilder.DropColumn(
                name: "ContactNumber",
                table: "AssetOrders");

            migrationBuilder.DropColumn(
                name: "DeliveryAddress",
                table: "AssetOrders");

            migrationBuilder.AlterColumn<int>(
                name: "CatalogItemId",
                table: "ClientAssets",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);
        }
    }
}
