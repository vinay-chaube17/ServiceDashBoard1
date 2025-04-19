using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ServiceDashBoard1.Migrations
{
    /// <inheritdoc />
    public partial class init3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SelectedMainProblem",
                table: "ComplaintRegistration");

            migrationBuilder.DropColumn(
                name: "SelectedSubProblem",
                table: "ComplaintRegistration");

            migrationBuilder.AddColumn<string>(
                name: "SelectedMainProblems",
                table: "ComplaintRegistration",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "SelectedSubProblems",
                table: "ComplaintRegistration",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SelectedMainProblems",
                table: "ComplaintRegistration");

            migrationBuilder.DropColumn(
                name: "SelectedSubProblems",
                table: "ComplaintRegistration");

            migrationBuilder.AddColumn<int>(
                name: "SelectedMainProblem",
                table: "ComplaintRegistration",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "SelectedSubProblem",
                table: "ComplaintRegistration",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
