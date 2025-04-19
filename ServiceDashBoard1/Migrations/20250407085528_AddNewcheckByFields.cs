using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ServiceDashBoard1.Migrations
{
    /// <inheritdoc />
    public partial class AddNewcheckByFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FinalRemarkBy",
                table: "ServiceModels",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RemarkBy",
                table: "ServiceModels",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FinalRemarkBy",
                table: "ServiceModels");

            migrationBuilder.DropColumn(
                name: "RemarkBy",
                table: "ServiceModels");
        }
    }
}
