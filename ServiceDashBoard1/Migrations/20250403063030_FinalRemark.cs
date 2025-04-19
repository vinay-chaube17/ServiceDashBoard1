using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ServiceDashBoard1.Migrations
{
    /// <inheritdoc />
    public partial class FinalRemark : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FinalRemark",
                table: "ServiceModels",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FinalRemark",
                table: "ServiceModels");
        }
    }
}
