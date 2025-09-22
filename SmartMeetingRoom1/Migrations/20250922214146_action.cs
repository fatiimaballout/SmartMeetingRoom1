using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartMeetingRoom1.Migrations
{
    /// <inheritdoc />
    public partial class action : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AssigneeUserId",
                table: "ActionItems",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CompletedUtc",
                table: "ActionItems",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedUtc",
                table: "ActionItems",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.CreateIndex(
                name: "IX_ActionItems_AssigneeUserId",
                table: "ActionItems",
                column: "AssigneeUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_ActionItems_AspNetUsers_AssigneeUserId",
                table: "ActionItems",
                column: "AssigneeUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ActionItems_AspNetUsers_AssigneeUserId",
                table: "ActionItems");

            migrationBuilder.DropIndex(
                name: "IX_ActionItems_AssigneeUserId",
                table: "ActionItems");

            migrationBuilder.DropColumn(
                name: "AssigneeUserId",
                table: "ActionItems");

            migrationBuilder.DropColumn(
                name: "CompletedUtc",
                table: "ActionItems");

            migrationBuilder.DropColumn(
                name: "CreatedUtc",
                table: "ActionItems");
        }
    }
}
