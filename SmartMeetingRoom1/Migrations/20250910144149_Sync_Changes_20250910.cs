using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartMeetingRoom1.Migrations
{
    /// <inheritdoc />
    public partial class Sync_Changes_20250910 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ActionItems_Users_AssignedTo",
                table: "ActionItems");

            migrationBuilder.DropForeignKey(
                name: "FK_ActionItems_Users_UserId",
                table: "ActionItems");

            migrationBuilder.DropForeignKey(
                name: "FK_Attachments_Users_UploaderId",
                table: "Attachments");

            migrationBuilder.DropForeignKey(
                name: "FK_MeetingAttendees_Users_UserId",
                table: "MeetingAttendees");

            migrationBuilder.DropForeignKey(
                name: "FK_Meetings_Users_UserId",
                table: "Meetings");

            migrationBuilder.DropForeignKey(
                name: "FK_Minutes_Users_CreatorId",
                table: "Minutes");

            migrationBuilder.DropForeignKey(
                name: "FK_Minutes_Users_UserId",
                table: "Minutes");

            migrationBuilder.DropForeignKey(
                name: "FK_Minutes_Users_UserId1",
                table: "Minutes");

            migrationBuilder.DropForeignKey(
                name: "FK_Notifications_Users_UserId",
                table: "Notifications");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Minutes_UserId1",
                table: "Minutes");

            migrationBuilder.DropIndex(
                name: "IX_Meetings_UserId",
                table: "Meetings");

            migrationBuilder.DropIndex(
                name: "IX_ActionItems_UserId",
                table: "ActionItems");

            migrationBuilder.DropColumn(
                name: "UserId1",
                table: "Minutes");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Meetings");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "ActionItems");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "Minutes",
                newName: "CreatedUtc");

            migrationBuilder.AddColumn<string>(
                name: "Notes",
                table: "Minutes",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<DateTime>(
                name: "DueDate",
                table: "ActionItems",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AlterColumn<int>(
                name: "AssignedTo",
                table: "ActionItems",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.CreateTable(
                name: "DomainUsers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Password = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Phone = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Role = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DomainUsers", x => x.Id);
                });

            migrationBuilder.AddForeignKey(
                name: "FK_ActionItems_DomainUsers_AssignedTo",
                table: "ActionItems",
                column: "AssignedTo",
                principalTable: "DomainUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Attachments_DomainUsers_UploaderId",
                table: "Attachments",
                column: "UploaderId",
                principalTable: "DomainUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_MeetingAttendees_DomainUsers_UserId",
                table: "MeetingAttendees",
                column: "UserId",
                principalTable: "DomainUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Minutes_AspNetUsers_CreatorId",
                table: "Minutes",
                column: "CreatorId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Minutes_DomainUsers_UserId",
                table: "Minutes",
                column: "UserId",
                principalTable: "DomainUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Notifications_DomainUsers_UserId",
                table: "Notifications",
                column: "UserId",
                principalTable: "DomainUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ActionItems_DomainUsers_AssignedTo",
                table: "ActionItems");

            migrationBuilder.DropForeignKey(
                name: "FK_Attachments_DomainUsers_UploaderId",
                table: "Attachments");

            migrationBuilder.DropForeignKey(
                name: "FK_MeetingAttendees_DomainUsers_UserId",
                table: "MeetingAttendees");

            migrationBuilder.DropForeignKey(
                name: "FK_Minutes_AspNetUsers_CreatorId",
                table: "Minutes");

            migrationBuilder.DropForeignKey(
                name: "FK_Minutes_DomainUsers_UserId",
                table: "Minutes");

            migrationBuilder.DropForeignKey(
                name: "FK_Notifications_DomainUsers_UserId",
                table: "Notifications");

            migrationBuilder.DropTable(
                name: "DomainUsers");

            migrationBuilder.DropColumn(
                name: "Notes",
                table: "Minutes");

            migrationBuilder.RenameColumn(
                name: "CreatedUtc",
                table: "Minutes",
                newName: "CreatedAt");

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

            migrationBuilder.AlterColumn<DateTime>(
                name: "DueDate",
                table: "ActionItems",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "AssignedTo",
                table: "ActionItems",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "UserId",
                table: "ActionItems",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Password = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Phone = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Role = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Minutes_UserId1",
                table: "Minutes",
                column: "UserId1");

            migrationBuilder.CreateIndex(
                name: "IX_Meetings_UserId",
                table: "Meetings",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_ActionItems_UserId",
                table: "ActionItems",
                column: "UserId");

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

            migrationBuilder.AddForeignKey(
                name: "FK_Attachments_Users_UploaderId",
                table: "Attachments",
                column: "UploaderId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_MeetingAttendees_Users_UserId",
                table: "MeetingAttendees",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Meetings_Users_UserId",
                table: "Meetings",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Minutes_Users_CreatorId",
                table: "Minutes",
                column: "CreatorId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Minutes_Users_UserId",
                table: "Minutes",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Minutes_Users_UserId1",
                table: "Minutes",
                column: "UserId1",
                principalTable: "Users",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Notifications_Users_UserId",
                table: "Notifications",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
