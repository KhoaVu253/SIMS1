using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SIMS.Migrations
{
    /// <inheritdoc />
    public partial class AddFacultyIdToCourseScheduleWithData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Step 1: Add column as nullable first
            migrationBuilder.AddColumn<int>(
                name: "FacultyId",
                table: "CourseSchedules",
                type: "int",
                nullable: true);

            // Step 2: Update existing records with Faculty from Course, or first available Faculty
            migrationBuilder.Sql(@"
                UPDATE cs
                SET cs.FacultyId = COALESCE(
                    c.FacultyId, 
                    (SELECT TOP 1 Id FROM Faculties WHERE IsActive = 1 ORDER BY Id)
                )
                FROM CourseSchedules cs
                INNER JOIN Courses c ON cs.CourseId = c.Id;
            ");

            // Step 3: Make column NOT NULL
            migrationBuilder.AlterColumn<int>(
                name: "FacultyId",
                table: "CourseSchedules",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            // Step 4: Create index
            migrationBuilder.CreateIndex(
                name: "IX_CourseSchedules_FacultyId",
                table: "CourseSchedules",
                column: "FacultyId");

            // Step 5: Add foreign key
            migrationBuilder.AddForeignKey(
                name: "FK_CourseSchedules_Faculties_FacultyId",
                table: "CourseSchedules",
                column: "FacultyId",
                principalTable: "Faculties",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CourseSchedules_Faculties_FacultyId",
                table: "CourseSchedules");

            migrationBuilder.DropIndex(
                name: "IX_CourseSchedules_FacultyId",
                table: "CourseSchedules");

            migrationBuilder.DropColumn(
                name: "FacultyId",
                table: "CourseSchedules");
        }
    }
}
