using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartMeetingRoom1.Migrations
{
    /// <inheritdoc />
    public partial class entityplus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ActionItem_Minutes_MinuteId",
                table: "ActionItem");

            migrationBuilder.DropForeignKey(
                name: "FK_ActionItem_Users_AssigneeId",
                table: "ActionItem");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ActionItem",
                table: "ActionItem");

            migrationBuilder.DropIndex(
                name: "IX_ActionItem_AssigneeId",
                table: "ActionItem");

            migrationBuilder.DropIndex(
                name: "IX_ActionItem_MinuteId",
                table: "ActionItem");

            migrationBuilder.DropColumn(
                name: "AssigneeId",
                table: "ActionItem");

            migrationBuilder.DropColumn(
                name: "MinuteId",
                table: "ActionItem");

            migrationBuilder.RenameTable(
                name: "ActionItem",
                newName: "ActionItems");

            migrationBuilder.AddColumn<int>(
                name: "UserId",
                table: "ActionItems",
                type: "int",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_ActionItems",
                table: "ActionItems",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_ActionItems_AssignedTo",
                table: "ActionItems",
                column: "AssignedTo");

            migrationBuilder.CreateIndex(
                name: "IX_ActionItems_MinutesId",
                table: "ActionItems",
                column: "MinutesId");

            migrationBuilder.CreateIndex(
                name: "IX_ActionItems_UserId",
                table: "ActionItems",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_ActionItems_Minutes_MinutesId",
                table: "ActionItems",
                column: "MinutesId",
                principalTable: "Minutes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ActionItems_Users_AssignedTo",
                table: "ActionItems",
                column: "AssignedTo",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ActionItems_Users_UserId",
                table: "ActionItems",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ActionItems_Minutes_MinutesId",
                table: "ActionItems");

            migrationBuilder.DropForeignKey(
                name: "FK_ActionItems_Users_AssignedTo",
                table: "ActionItems");

            migrationBuilder.DropForeignKey(
                name: "FK_ActionItems_Users_UserId",
                table: "ActionItems");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ActionItems",
                table: "ActionItems");

            migrationBuilder.DropIndex(
                name: "IX_ActionItems_AssignedTo",
                table: "ActionItems");

            migrationBuilder.DropIndex(
                name: "IX_ActionItems_MinutesId",
                table: "ActionItems");

            migrationBuilder.DropIndex(
                name: "IX_ActionItems_UserId",
                table: "ActionItems");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "ActionItems");

            migrationBuilder.RenameTable(
                name: "ActionItems",
                newName: "ActionItem");

            migrationBuilder.AddColumn<int>(
                name: "AssigneeId",
                table: "ActionItem",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "MinuteId",
                table: "ActionItem",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddPrimaryKey(
                name: "PK_ActionItem",
                table: "ActionItem",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_ActionItem_AssigneeId",
                table: "ActionItem",
                column: "AssigneeId");

            migrationBuilder.CreateIndex(
                name: "IX_ActionItem_MinuteId",
                table: "ActionItem",
                column: "MinuteId");

            migrationBuilder.AddForeignKey(
                name: "FK_ActionItem_Minutes_MinuteId",
                table: "ActionItem",
                column: "MinuteId",
                principalTable: "Minutes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ActionItem_Users_AssigneeId",
                table: "ActionItem",
                column: "AssigneeId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
