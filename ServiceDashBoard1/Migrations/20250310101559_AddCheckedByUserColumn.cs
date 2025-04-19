using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ServiceDashBoard1.Migrations
{
    /// <inheritdoc />
    public partial class AddCheckedByUserColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CheckedBy",
                table: "ComplaintRegistration",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CheckedBy",
                table: "ComplaintRegistration");
        }
    }
}
