using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SignMate.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCourseCenterId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CenterId",
                table: "Courses",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Courses_CenterId",
                table: "Courses",
                column: "CenterId");

            migrationBuilder.AddForeignKey(
                name: "FK_Courses_Centers_CenterId",
                table: "Courses",
                column: "CenterId",
                principalTable: "Centers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Courses_Centers_CenterId",
                table: "Courses");

            migrationBuilder.DropIndex(
                name: "IX_Courses_CenterId",
                table: "Courses");

            migrationBuilder.DropColumn(
                name: "CenterId",
                table: "Courses");
        }
    }
}
