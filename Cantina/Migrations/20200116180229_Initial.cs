using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace Cantina.Migrations
{
    public partial class Initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Email = table.Column<string>(maxLength: 64, nullable: false),
                    Name = table.Column<string>(maxLength: 20, nullable: false),
                    Confirmed = table.Column<bool>(nullable: false),
                    Active = table.Column<bool>(nullable: false),
                    Role = table.Column<byte>(nullable: false),
                    Profile_Gender = table.Column<byte>(nullable: true),
                    Profile_Location = table.Column<string>(maxLength: 32, nullable: true),
                    Profile_Birthday = table.Column<DateTime>(nullable: true),
                    Profile_RegisterDate = table.Column<DateTime>(type: "date", nullable: false),
                    Profile_LastEnterDate = table.Column<DateTime>(type: "date", nullable: true),
                    Profile_OnlineTime = table.Column<int>(nullable: true),
                    Profile_MessageStyle = table.Column<string>(nullable: true),
                    PasswordHash = table.Column<string>(maxLength: 128, nullable: false),
                    salt = table.Column<string>(maxLength: 64, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                    table.UniqueConstraint("AK_Users_Email_Name", x => new { x.Email, x.Name });
                });

            migrationBuilder.CreateTable(
                name: "History",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Date = table.Column<DateTime>(nullable: false),
                    Type = table.Column<byte>(nullable: false),
                    Description = table.Column<string>(maxLength: 255, nullable: true),
                    UserID = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_History", x => x.Id);
                    table.ForeignKey(
                        name: "FK_History_Users_UserID",
                        column: x => x.UserID,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_History_UserID",
                table: "History",
                column: "UserID");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "History");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
