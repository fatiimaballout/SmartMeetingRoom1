using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartMeetingRoom1.Migrations
{
    /// <inheritdoc />
    public partial class SyncManualChange : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ActionItems_DomainUsers_AssignedTo",
                table: "ActionItems");

            migrationBuilder.DropIndex(
                name: "IX_ActionItems_AssignedTo",
                table: "ActionItems");

            migrationBuilder.AlterColumn<string>(
                name: "AssignedTo",
                table: "ActionItems",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "UserId",
                table: "ActionItems",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ActionItems_UserId",
                table: "ActionItems",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_ActionItems_DomainUsers_UserId",
                table: "ActionItems",
                column: "UserId",
                principalTable: "DomainUsers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ActionItems_DomainUsers_UserId",
                table: "ActionItems");

            migrationBuilder.DropIndex(
                name: "IX_ActionItems_UserId",
                table: "ActionItems");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "ActionItems");

            migrationBuilder.AlterColumn<int>(
                name: "AssignedTo",
                table: "ActionItems",
                type: "int",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ActionItems_AssignedTo",
                table: "ActionItems",
                column: "AssignedTo");

            migrationBuilder.AddForeignKey(
                name: "FK_ActionItems_DomainUsers_AssignedTo",
                table: "ActionItems",
                column: "AssignedTo",
                principalTable: "DomainUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
