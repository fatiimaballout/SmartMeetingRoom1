using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartMeetingRoom1.Migrations
{
    /// <inheritdoc />
    public partial class SyncSnapshot : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Attachments_DomainUsers_UploaderId",
                table: "Attachments");

            migrationBuilder.AlterColumn<int>(
                name: "UploaderId",
                table: "Attachments",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<string>(
                name: "FileName",
                table: "Attachments",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<byte[]>(
                name: "FileContent",
                table: "Attachments",
                type: "varbinary(max)",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "SizeBytes",
                table: "Attachments",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddForeignKey(
                name: "FK_Attachments_DomainUsers_UploaderId",
                table: "Attachments",
                column: "UploaderId",
                principalTable: "DomainUsers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Attachments_DomainUsers_UploaderId",
                table: "Attachments");

            migrationBuilder.DropColumn(
                name: "FileContent",
                table: "Attachments");

            migrationBuilder.DropColumn(
                name: "SizeBytes",
                table: "Attachments");

            migrationBuilder.AlterColumn<int>(
                name: "UploaderId",
                table: "Attachments",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "FileName",
                table: "Attachments",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Attachments_DomainUsers_UploaderId",
                table: "Attachments",
                column: "UploaderId",
                principalTable: "DomainUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
