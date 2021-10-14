using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Scani.Database.Migrations
{
    public partial class FixAutomaticEnumConfiguration : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Users_UserRoles_UserRoleId",
                table: "Users");

            migrationBuilder.DropTable(
                name: "UserRoles");

            migrationBuilder.DropIndex(
                name: "IX_Users_UserRoleId",
                table: "Users");

            migrationBuilder.AddColumn<int>(
                name: "RoleId",
                table: "Users",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "UserRoleEnum",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: false),
                    Ordinal = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserRoleEnum", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "UserRoleEnum",
                columns: new[] { "Id", "Description", "Name", "Ordinal" },
                values: new object[] { 1, "student", "Student", 1 });

            migrationBuilder.InsertData(
                table: "UserRoleEnum",
                columns: new[] { "Id", "Description", "Name", "Ordinal" },
                values: new object[] { 2, "teacher", "Teacher", 2 });

            migrationBuilder.CreateIndex(
                name: "IX_Users_RoleId",
                table: "Users",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_UserRoleEnum_Description",
                table: "UserRoleEnum",
                column: "Description",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserRoleEnum_Name",
                table: "UserRoleEnum",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserRoleEnum_Ordinal",
                table: "UserRoleEnum",
                column: "Ordinal",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Users_UserRoleEnum_RoleId",
                table: "Users",
                column: "RoleId",
                principalTable: "UserRoleEnum",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Users_UserRoleEnum_RoleId",
                table: "Users");

            migrationBuilder.DropTable(
                name: "UserRoleEnum");

            migrationBuilder.DropIndex(
                name: "IX_Users_RoleId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "RoleId",
                table: "Users");

            migrationBuilder.CreateTable(
                name: "UserRoles",
                columns: table => new
                {
                    UserRoleId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Description = table.Column<string>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Ordinal = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserRoles", x => x.UserRoleId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Users_UserRoleId",
                table: "Users",
                column: "UserRoleId");

            migrationBuilder.CreateIndex(
                name: "IX_UserRoles_Description",
                table: "UserRoles",
                column: "Description",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserRoles_Name",
                table: "UserRoles",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserRoles_Ordinal",
                table: "UserRoles",
                column: "Ordinal",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Users_UserRoles_UserRoleId",
                table: "Users",
                column: "UserRoleId",
                principalTable: "UserRoles",
                principalColumn: "UserRoleId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
