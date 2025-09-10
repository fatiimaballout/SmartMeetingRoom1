using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartMeetingRoom1.Migrations
{
    /// <inheritdoc />
    public partial class SyncModel_20250909 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Meetings_Users_OrganizerId",
                table: "Meetings");

            migrationBuilder.AddColumn<int>(
                name: "UserId1",
                table: "Minutes",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "UserId",
                table: "Meetings",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Minutes_UserId1",
                table: "Minutes",
                column: "UserId1");

            migrationBuilder.CreateIndex(
                name: "IX_Meetings_UserId",
                table: "Meetings",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Meetings_AspNetUsers_OrganizerId",
                table: "Meetings",
                column: "OrganizerId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Meetings_Users_UserId",
                table: "Meetings",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Minutes_Users_UserId1",
                table: "Minutes",
                column: "UserId1",
                principalTable: "Users",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Meetings_AspNetUsers_OrganizerId",
                table: "Meetings");

            migrationBuilder.DropForeignKey(
                name: "FK_Meetings_Users_UserId",
                table: "Meetings");

            migrationBuilder.DropForeignKey(
                name: "FK_Minutes_Users_UserId1",
                table: "Minutes");

            migrationBuilder.DropIndex(
                name: "IX_Minutes_UserId1",
                table: "Minutes");

            migrationBuilder.DropIndex(
                name: "IX_Meetings_UserId",
                table: "Meetings");

            migrationBuilder.DropColumn(
                name: "UserId1",
                table: "Minutes");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Meetings");

            migrationBuilder.AddForeignKey(
                name: "FK_Meetings_Users_OrganizerId",
                table: "Meetings",
                column: "OrganizerId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
