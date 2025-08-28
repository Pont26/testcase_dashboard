using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TestCaseDashboard.Migrations
{
    /// <inheritdoc />
    public partial class UpdateBuglistRelationship : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "public");

            migrationBuilder.CreateTable(
                name: "project",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    projectname = table.Column<string>(type: "text", nullable: false),
                    source = table.Column<int>(type: "integer", nullable: false),
                    createdat = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    updatedat = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_project", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "teammember",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false),
                    createdat = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    updatedat = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_teammember", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "testcase",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    screen = table.Column<string>(type: "text", nullable: false),
                    function = table.Column<string>(type: "text", nullable: true),
                    projectid = table.Column<Guid>(type: "uuid", nullable: false),
                    createdat = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    updatedat = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_testcase", x => x.id);
                    table.ForeignKey(
                        name: "FK_testcase_project_projectid",
                        column: x => x.projectid,
                        principalSchema: "public",
                        principalTable: "project",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "project_teammember",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    teammemberid = table.Column<Guid>(type: "uuid", nullable: false),
                    projectid = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_project_teammember", x => x.id);
                    table.ForeignKey(
                        name: "FK_project_teammember_project_projectid",
                        column: x => x.projectid,
                        principalSchema: "public",
                        principalTable: "project",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_project_teammember_teammember_teammemberid",
                        column: x => x.teammemberid,
                        principalSchema: "public",
                        principalTable: "teammember",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "testcase_teammember",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    teammemberid = table.Column<Guid>(type: "uuid", nullable: false),
                    role = table.Column<int>(type: "integer", nullable: false),
                    testStatus = table.Column<int>(type: "integer", nullable: false),
                    testcaseid = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_testcase_teammember", x => x.id);
                    table.ForeignKey(
                        name: "FK_testcase_teammember_teammember_teammemberid",
                        column: x => x.teammemberid,
                        principalSchema: "public",
                        principalTable: "teammember",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_testcase_teammember_testcase_testcaseid",
                        column: x => x.testcaseid,
                        principalSchema: "public",
                        principalTable: "testcase",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "buglist",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    testmemberid = table.Column<Guid>(type: "uuid", nullable: false),
                    remark = table.Column<string>(type: "text", nullable: true),
                    image = table.Column<string>(type: "text", nullable: true),
                    createdat = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    updatedat = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_buglist", x => x.id);
                    table.ForeignKey(
                        name: "FK_buglist_testcase_teammember_testmemberid",
                        column: x => x.testmemberid,
                        principalSchema: "public",
                        principalTable: "testcase_teammember",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_buglist_testmemberid",
                schema: "public",
                table: "buglist",
                column: "testmemberid");

            migrationBuilder.CreateIndex(
                name: "IX_project_teammember_projectid",
                schema: "public",
                table: "project_teammember",
                column: "projectid");

            migrationBuilder.CreateIndex(
                name: "IX_project_teammember_teammemberid",
                schema: "public",
                table: "project_teammember",
                column: "teammemberid");

            migrationBuilder.CreateIndex(
                name: "IX_testcase_projectid",
                schema: "public",
                table: "testcase",
                column: "projectid");

            migrationBuilder.CreateIndex(
                name: "IX_testcase_teammember_teammemberid",
                schema: "public",
                table: "testcase_teammember",
                column: "teammemberid");

            migrationBuilder.CreateIndex(
                name: "IX_testcase_teammember_testcaseid",
                schema: "public",
                table: "testcase_teammember",
                column: "testcaseid");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "buglist",
                schema: "public");

            migrationBuilder.DropTable(
                name: "project_teammember",
                schema: "public");

            migrationBuilder.DropTable(
                name: "testcase_teammember",
                schema: "public");

            migrationBuilder.DropTable(
                name: "teammember",
                schema: "public");

            migrationBuilder.DropTable(
                name: "testcase",
                schema: "public");

            migrationBuilder.DropTable(
                name: "project",
                schema: "public");
        }
    }
}
