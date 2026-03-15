using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AIStudyPlanner.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPracticeTests : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PracticeTests",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    UserId = table.Column<string>(type: "TEXT", nullable: false),
                    SubjectId = table.Column<Guid>(type: "TEXT", nullable: false),
                    SubjectName = table.Column<string>(type: "TEXT", nullable: false),
                    Score = table.Column<int>(type: "INTEGER", nullable: false),
                    TotalQuestions = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PracticeTests", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PracticeQuestions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    PracticeTestId = table.Column<Guid>(type: "TEXT", nullable: false),
                    QuestionText = table.Column<string>(type: "TEXT", nullable: false),
                    OptionA = table.Column<string>(type: "TEXT", nullable: false),
                    OptionB = table.Column<string>(type: "TEXT", nullable: false),
                    OptionC = table.Column<string>(type: "TEXT", nullable: false),
                    OptionD = table.Column<string>(type: "TEXT", nullable: false),
                    CorrectAnswer = table.Column<string>(type: "TEXT", nullable: false),
                    UserAnswer = table.Column<string>(type: "TEXT", nullable: false),
                    Explanation = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PracticeQuestions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PracticeQuestions_PracticeTests_PracticeTestId",
                        column: x => x.PracticeTestId,
                        principalTable: "PracticeTests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PracticeQuestions_PracticeTestId",
                table: "PracticeQuestions",
                column: "PracticeTestId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PracticeQuestions");

            migrationBuilder.DropTable(
                name: "PracticeTests");
        }
    }
}
