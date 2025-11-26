using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ToroTrack.Migrations
{
    /// <inheritdoc />
    public partial class AddAssignedTeamColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ClientAssets_AspNetUsers_ClientId",
                table: "ClientAssets");

            migrationBuilder.DropForeignKey(
                name: "FK_ClientAssets_CatalogItems_CatalogItemId",
                table: "ClientAssets");

            migrationBuilder.DropForeignKey(
                name: "FK_ProjectTasks_AspNetUsers_AssignedToId",
                table: "ProjectTasks");

            migrationBuilder.DropIndex(
                name: "IX_ProjectTasks_AssignedToId",
                table: "ProjectTasks");

            migrationBuilder.DropIndex(
                name: "IX_ClientAssets_CatalogItemId",
                table: "ClientAssets");

            migrationBuilder.DropIndex(
                name: "IX_ClientAssets_ClientId",
                table: "ClientAssets");

            migrationBuilder.AlterColumn<string>(
                name: "AssignedToId",
                table: "ProjectTasks",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CompletedDate",
                table: "ProjectTasks",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Priority",
                table: "ProjectTasks",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "AssignedTeam",
                table: "Projects",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CompletedDate",
                table: "ProjectTasks");

            migrationBuilder.DropColumn(
                name: "Priority",
                table: "ProjectTasks");

            migrationBuilder.DropColumn(
                name: "AssignedTeam",
                table: "Projects");

            migrationBuilder.AlterColumn<string>(
                name: "AssignedToId",
                table: "ProjectTasks",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectTasks_AssignedToId",
                table: "ProjectTasks",
                column: "AssignedToId");

            migrationBuilder.CreateIndex(
                name: "IX_ClientAssets_CatalogItemId",
                table: "ClientAssets",
                column: "CatalogItemId");

            migrationBuilder.CreateIndex(
                name: "IX_ClientAssets_ClientId",
                table: "ClientAssets",
                column: "ClientId");

            migrationBuilder.AddForeignKey(
                name: "FK_ClientAssets_AspNetUsers_ClientId",
                table: "ClientAssets",
                column: "ClientId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ClientAssets_CatalogItems_CatalogItemId",
                table: "ClientAssets",
                column: "CatalogItemId",
                principalTable: "CatalogItems",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ProjectTasks_AspNetUsers_AssignedToId",
                table: "ProjectTasks",
                column: "AssignedToId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }
    }
}
