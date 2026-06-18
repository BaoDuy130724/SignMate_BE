using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SignMate.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddJudgeToPracticeAttempt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "JudgeRubricJson",
                table: "PracticeAttempts",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "JudgeVerdict",
                table: "PracticeAttempts",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "JudgeRubricJson",
                table: "PracticeAttempts");

            migrationBuilder.DropColumn(
                name: "JudgeVerdict",
                table: "PracticeAttempts");
        }
    }
}
