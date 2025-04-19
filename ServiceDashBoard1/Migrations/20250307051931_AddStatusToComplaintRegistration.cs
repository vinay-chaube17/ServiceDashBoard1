using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ServiceDashBoard1.Migrations
{
    /// <inheritdoc />
    public partial class AddStatusToComplaintRegistration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "ComplaintRegistration",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Status",
                table: "ComplaintRegistration");
        }
    }
}
