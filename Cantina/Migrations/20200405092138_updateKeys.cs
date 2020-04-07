using Microsoft.EntityFrameworkCore.Migrations;

namespace Cantina.Migrations
{
    public partial class updateKeys : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropUniqueConstraint(
                name: "AK_UserProfiles_Name",
                table: "UserProfiles");

            migrationBuilder.DropUniqueConstraint(
                name: "AK_ForbiddenNames_Name",
                table: "ForbiddenNames");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "UserProfiles",
                maxLength: 20,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(20)",
                oldMaxLength: 20);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "ForbiddenNames",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.CreateIndex(
                name: "IX_UserProfiles_Name",
                table: "UserProfiles",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ForbiddenNames_Name",
                table: "ForbiddenNames",
                column: "Name",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_UserProfiles_Name",
                table: "UserProfiles");

            migrationBuilder.DropIndex(
                name: "IX_ForbiddenNames_Name",
                table: "ForbiddenNames");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "UserProfiles",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(string),
                oldMaxLength: 20,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "ForbiddenNames",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.AddUniqueConstraint(
                name: "AK_UserProfiles_Name",
                table: "UserProfiles",
                column: "Name");

            migrationBuilder.AddUniqueConstraint(
                name: "AK_ForbiddenNames_Name",
                table: "ForbiddenNames",
                column: "Name");
        }
    }
}
