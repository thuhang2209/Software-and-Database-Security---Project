using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Software_and_Database_Security___Project.Migrations
{
    /// <inheritdoc />
    public partial class UpdateDatabase : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SubmissionFileName",
                table: "Submissions",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SubmissionFilePath",
                table: "Submissions",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SubmissionFileName",
                table: "Submissions");

            migrationBuilder.DropColumn(
                name: "SubmissionFilePath",
                table: "Submissions");
        }
    }
}
