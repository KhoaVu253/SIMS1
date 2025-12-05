using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SIMS.Migrations
{
    /// <inheritdoc />
    public partial class UpdateEnrollmentWithGrades : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Grades");

            migrationBuilder.RenameColumn(
                name: "EnrolledAt",
                table: "Enrollments",
                newName: "EnrollmentDate");

            migrationBuilder.AddColumn<int>(
                name: "AssignedByUserId",
                table: "Enrollments",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "AssignedDate",
                table: "Enrollments",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<float>(
                name: "AverageScore",
                table: "Enrollments",
                type: "real",
                nullable: true);

            migrationBuilder.AddColumn<float>(
                name: "FinalScore",
                table: "Enrollments",
                type: "real",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LetterGrade",
                table: "Enrollments",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<float>(
                name: "MidtermScore",
                table: "Enrollments",
                type: "real",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Notes",
                table: "Enrollments",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "Enrollments",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Enrollments_AssignedByUserId",
                table: "Enrollments",
                column: "AssignedByUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Enrollments_Users_AssignedByUserId",
                table: "Enrollments",
                column: "AssignedByUserId",
                principalTable: "Users",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Enrollments_Users_AssignedByUserId",
                table: "Enrollments");

            migrationBuilder.DropIndex(
                name: "IX_Enrollments_AssignedByUserId",
                table: "Enrollments");

            migrationBuilder.DropColumn(
                name: "AssignedByUserId",
                table: "Enrollments");

            migrationBuilder.DropColumn(
                name: "AssignedDate",
                table: "Enrollments");

            migrationBuilder.DropColumn(
                name: "AverageScore",
                table: "Enrollments");

            migrationBuilder.DropColumn(
                name: "FinalScore",
                table: "Enrollments");

            migrationBuilder.DropColumn(
                name: "LetterGrade",
                table: "Enrollments");

            migrationBuilder.DropColumn(
                name: "MidtermScore",
                table: "Enrollments");

            migrationBuilder.DropColumn(
                name: "Notes",
                table: "Enrollments");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Enrollments");

            migrationBuilder.RenameColumn(
                name: "EnrollmentDate",
                table: "Enrollments",
                newName: "EnrolledAt");

            migrationBuilder.CreateTable(
                name: "Grades",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EnrollmentId = table.Column<int>(type: "int", nullable: false),
                    FinalScore = table.Column<float>(type: "real", nullable: true),
                    LetterGrade = table.Column<string>(type: "nvarchar(5)", maxLength: 5, nullable: true),
                    MidtermScore = table.Column<float>(type: "real", nullable: true),
                    TotalScore = table.Column<float>(type: "real", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Grades", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Grades_Enrollments_EnrollmentId",
                        column: x => x.EnrollmentId,
                        principalTable: "Enrollments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Grades_EnrollmentId",
                table: "Grades",
                column: "EnrollmentId",
                unique: true);
        }
    }
}
