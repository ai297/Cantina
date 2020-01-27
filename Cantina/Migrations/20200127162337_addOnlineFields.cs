using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Cantina.Migrations
{
    public partial class addOnlineFields : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Profile_LastEnterDate",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "Profile_RegisterDate",
                table: "Users");

            migrationBuilder.AddColumn<DateTime>(
                name: "LastEnterTime",
                table: "Users",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LastEnterTime",
                table: "Users");

            migrationBuilder.AddColumn<DateTime>(
                name: "Profile_LastEnterDate",
                table: "Users",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "Profile_RegisterDate",
                table: "Users",
                type: "date",
                nullable: true);
        }
    }
}
