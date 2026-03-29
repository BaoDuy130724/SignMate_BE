using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SignMate.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSignReferenceRequest : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SignReferenceRequests",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SignId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UploaderId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    VideoUrl = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ExtractedKeypoints = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ReviewedById = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ReviewedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ReviewComment = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SignReferenceRequests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SignReferenceRequests_Signs_SignId",
                        column: x => x.SignId,
                        principalTable: "Signs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SignReferenceRequests_Users_ReviewedById",
                        column: x => x.ReviewedById,
                        principalTable: "Users",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_SignReferenceRequests_Users_UploaderId",
                        column: x => x.UploaderId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SignReferenceRequests_ReviewedById",
                table: "SignReferenceRequests",
                column: "ReviewedById");

            migrationBuilder.CreateIndex(
                name: "IX_SignReferenceRequests_SignId",
                table: "SignReferenceRequests",
                column: "SignId");

            migrationBuilder.CreateIndex(
                name: "IX_SignReferenceRequests_UploaderId",
                table: "SignReferenceRequests",
                column: "UploaderId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SignReferenceRequests");
        }
    }
}
