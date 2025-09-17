using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartMeetingRoom1.Migrations
{
    /// <inheritdoc />
    public partial class changes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MeetingAttendees_DomainUsers_UserId",
                table: "MeetingAttendees");

            migrationBuilder.DropPrimaryKey(
                name: "PK_MeetingAttendees",
                table: "MeetingAttendees");

            migrationBuilder.DropColumn(
                name: "Email",
                table: "MeetingAttendees");

            migrationBuilder.AddPrimaryKey(
                name: "PK_MeetingAttendees",
                table: "MeetingAttendees",
                columns: new[] { "MeetingId", "UserId" });

            migrationBuilder.AddForeignKey(
                name: "FK_MeetingAttendees_AspNetUsers_UserId",
                table: "MeetingAttendees",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MeetingAttendees_AspNetUsers_UserId",
                table: "MeetingAttendees");

            migrationBuilder.DropPrimaryKey(
                name: "PK_MeetingAttendees",
                table: "MeetingAttendees");

            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "MeetingAttendees",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddPrimaryKey(
                name: "PK_MeetingAttendees",
                table: "MeetingAttendees",
                columns: new[] { "MeetingId", "Email" });

            migrationBuilder.AddForeignKey(
                name: "FK_MeetingAttendees_DomainUsers_UserId",
                table: "MeetingAttendees",
                column: "UserId",
                principalTable: "DomainUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
