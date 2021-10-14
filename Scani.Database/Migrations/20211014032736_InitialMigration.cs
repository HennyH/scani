using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Scani.Database.Migrations
{
    public partial class InitialMigration : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Class",
                columns: table => new
                {
                    ClassId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Class", x => x.ClassId);
                });

            migrationBuilder.CreateTable(
                name: "Items",
                columns: table => new
                {
                    ItemId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Items", x => x.ItemId);
                });

            migrationBuilder.CreateTable(
                name: "ItemSets",
                columns: table => new
                {
                    ItemSetId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ItemSets", x => x.ItemSetId);
                });

            migrationBuilder.CreateTable(
                name: "LoansGroups",
                columns: table => new
                {
                    LoanGroupId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LoansGroups", x => x.LoanGroupId);
                });

            migrationBuilder.CreateTable(
                name: "ScanCodes",
                columns: table => new
                {
                    ScanCodeId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    QrCodeBytes = table.Column<byte[]>(type: "BLOB", nullable: true),
                    BarCodeBytes = table.Column<byte[]>(type: "BLOB", nullable: true),
                    Text = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScanCodes", x => x.ScanCodeId);
                });

            migrationBuilder.CreateTable(
                name: "UserRoles",
                columns: table => new
                {
                    UserRoleId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: false),
                    Ordinal = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserRoles", x => x.UserRoleId);
                });

            migrationBuilder.CreateTable(
                name: "ClassStudentGroups",
                columns: table => new
                {
                    ClassStudentGroupId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ClassId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClassStudentGroups", x => x.ClassStudentGroupId);
                    table.ForeignKey(
                        name: "FK_ClassStudentGroups_Class_ClassId",
                        column: x => x.ClassId,
                        principalTable: "Class",
                        principalColumn: "ClassId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ClassTimes",
                columns: table => new
                {
                    ClassTimeId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ClassId = table.Column<int>(type: "INTEGER", nullable: false),
                    DayOfWeek = table.Column<int>(type: "INTEGER", nullable: false),
                    StartTime = table.Column<TimeOnly>(type: "TEXT", nullable: false),
                    DurationMinutes = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClassTimes", x => x.ClassTimeId);
                    table.ForeignKey(
                        name: "FK_ClassTimes_Class_ClassId",
                        column: x => x.ClassId,
                        principalTable: "Class",
                        principalColumn: "ClassId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ItemSetItems",
                columns: table => new
                {
                    ItemSetItemId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ItemSetId = table.Column<int>(type: "INTEGER", nullable: false),
                    ItemId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ItemSetItems", x => x.ItemSetItemId);
                    table.ForeignKey(
                        name: "FK_ItemSetItems_Items_ItemId",
                        column: x => x.ItemId,
                        principalTable: "Items",
                        principalColumn: "ItemId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ItemSetItems_ItemSets_ItemSetId",
                        column: x => x.ItemSetId,
                        principalTable: "ItemSets",
                        principalColumn: "ItemSetId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Loans",
                columns: table => new
                {
                    LoadId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    LoanGroupId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Loans", x => x.LoadId);
                    table.ForeignKey(
                        name: "FK_Loans_LoansGroups_LoanGroupId",
                        column: x => x.LoanGroupId,
                        principalTable: "LoansGroups",
                        principalColumn: "LoanGroupId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UserRoleId = table.Column<int>(type: "INTEGER", nullable: false),
                    Username = table.Column<string>(type: "TEXT", nullable: false),
                    SaltBytes = table.Column<byte[]>(type: "BLOB", nullable: false),
                    HashedPassword = table.Column<byte[]>(type: "BLOB", nullable: false),
                    ScanCodeId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.UserId);
                    table.ForeignKey(
                        name: "FK_Users_ScanCodes_ScanCodeId",
                        column: x => x.ScanCodeId,
                        principalTable: "ScanCodes",
                        principalColumn: "ScanCodeId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Users_UserRoles_UserRoleId",
                        column: x => x.UserRoleId,
                        principalTable: "UserRoles",
                        principalColumn: "UserRoleId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LoanItemLines",
                columns: table => new
                {
                    LoanItemLineId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    LoanId = table.Column<int>(type: "INTEGER", nullable: false),
                    ItemId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LoanItemLines", x => x.LoanItemLineId);
                    table.ForeignKey(
                        name: "FK_LoanItemLines_Items_ItemId",
                        column: x => x.ItemId,
                        principalTable: "Items",
                        principalColumn: "ItemId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LoanItemLines_Loans_LoanId",
                        column: x => x.LoanId,
                        principalTable: "Loans",
                        principalColumn: "LoadId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ClassStudents",
                columns: table => new
                {
                    ClassStudentId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ClassId = table.Column<int>(type: "INTEGER", nullable: false),
                    UserId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClassStudents", x => x.ClassStudentId);
                    table.ForeignKey(
                        name: "FK_ClassStudents_Class_ClassId",
                        column: x => x.ClassId,
                        principalTable: "Class",
                        principalColumn: "ClassId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ClassStudents_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ClassTeachers",
                columns: table => new
                {
                    ClassTeacherId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ClassId = table.Column<int>(type: "INTEGER", nullable: false),
                    UserId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClassTeachers", x => x.ClassTeacherId);
                    table.ForeignKey(
                        name: "FK_ClassTeachers_Class_ClassId",
                        column: x => x.ClassId,
                        principalTable: "Class",
                        principalColumn: "ClassId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ClassTeachers_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LoansGroupsMembers",
                columns: table => new
                {
                    LoanGroupMemberId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    LoanGroupId = table.Column<int>(type: "INTEGER", nullable: false),
                    UserId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LoansGroupsMembers", x => x.LoanGroupMemberId);
                    table.ForeignKey(
                        name: "FK_LoansGroupsMembers_LoansGroups_LoanGroupId",
                        column: x => x.LoanGroupId,
                        principalTable: "LoansGroups",
                        principalColumn: "LoanGroupId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LoansGroupsMembers_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ClassStudentGroupMembers",
                columns: table => new
                {
                    ClassStudentGroupMemberId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ClassStudentGroupId = table.Column<int>(type: "INTEGER", nullable: false),
                    ClassStudentId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClassStudentGroupMembers", x => x.ClassStudentGroupMemberId);
                    table.ForeignKey(
                        name: "FK_ClassStudentGroupMembers_ClassStudentGroups_ClassStudentGroupId",
                        column: x => x.ClassStudentGroupId,
                        principalTable: "ClassStudentGroups",
                        principalColumn: "ClassStudentGroupId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ClassStudentGroupMembers_ClassStudents_ClassStudentId",
                        column: x => x.ClassStudentId,
                        principalTable: "ClassStudents",
                        principalColumn: "ClassStudentId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Class_Name",
                table: "Class",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ClassStudentGroupMembers_ClassStudentGroupId_ClassStudentId",
                table: "ClassStudentGroupMembers",
                columns: new[] { "ClassStudentGroupId", "ClassStudentId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ClassStudentGroupMembers_ClassStudentId",
                table: "ClassStudentGroupMembers",
                column: "ClassStudentId");

            migrationBuilder.CreateIndex(
                name: "IX_ClassStudentGroups_ClassId",
                table: "ClassStudentGroups",
                column: "ClassId");

            migrationBuilder.CreateIndex(
                name: "IX_ClassStudents_ClassId_UserId",
                table: "ClassStudents",
                columns: new[] { "ClassId", "UserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ClassStudents_UserId",
                table: "ClassStudents",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_ClassTeachers_ClassId_UserId",
                table: "ClassTeachers",
                columns: new[] { "ClassId", "UserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ClassTeachers_UserId",
                table: "ClassTeachers",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_ClassTimes_ClassId_StartTime",
                table: "ClassTimes",
                columns: new[] { "ClassId", "StartTime" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Items_Name",
                table: "Items",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ItemSetItems_ItemId",
                table: "ItemSetItems",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_ItemSetItems_ItemSetId_ItemId",
                table: "ItemSetItems",
                columns: new[] { "ItemSetId", "ItemId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ItemSets_Name",
                table: "ItemSets",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LoanItemLines_ItemId",
                table: "LoanItemLines",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_LoanItemLines_LoanId_ItemId",
                table: "LoanItemLines",
                columns: new[] { "LoanId", "ItemId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Loans_LoanGroupId",
                table: "Loans",
                column: "LoanGroupId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LoansGroupsMembers_LoanGroupId",
                table: "LoansGroupsMembers",
                column: "LoanGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_LoansGroupsMembers_UserId",
                table: "LoansGroupsMembers",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_ScanCodes_Text",
                table: "ScanCodes",
                column: "Text",
                unique: true);

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

            migrationBuilder.CreateIndex(
                name: "IX_Users_ScanCodeId",
                table: "Users",
                column: "ScanCodeId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_UserRoleId",
                table: "Users",
                column: "UserRoleId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ClassStudentGroupMembers");

            migrationBuilder.DropTable(
                name: "ClassTeachers");

            migrationBuilder.DropTable(
                name: "ClassTimes");

            migrationBuilder.DropTable(
                name: "ItemSetItems");

            migrationBuilder.DropTable(
                name: "LoanItemLines");

            migrationBuilder.DropTable(
                name: "LoansGroupsMembers");

            migrationBuilder.DropTable(
                name: "ClassStudentGroups");

            migrationBuilder.DropTable(
                name: "ClassStudents");

            migrationBuilder.DropTable(
                name: "ItemSets");

            migrationBuilder.DropTable(
                name: "Items");

            migrationBuilder.DropTable(
                name: "Loans");

            migrationBuilder.DropTable(
                name: "Class");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "LoansGroups");

            migrationBuilder.DropTable(
                name: "ScanCodes");

            migrationBuilder.DropTable(
                name: "UserRoles");
        }
    }
}
