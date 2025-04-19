using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ServiceDashBoard1.Migrations
{
    /// <inheritdoc />
    public partial class AddForeignKey : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TokenId",
                table: "ComplaintRegistration",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_ComplaintRegistration_TokenId",
                table: "ComplaintRegistration",
                column: "TokenId");

            migrationBuilder.AddForeignKey(
                name: "FK_ComplaintRegistration_TokenSequences_TokenId",
                table: "ComplaintRegistration",
                column: "TokenId",
                principalTable: "TokenSequences",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ComplaintRegistration_TokenSequences_TokenId",
                table: "ComplaintRegistration");

            migrationBuilder.DropIndex(
                name: "IX_ComplaintRegistration_TokenId",
                table: "ComplaintRegistration");

            migrationBuilder.DropColumn(
                name: "TokenId",
                table: "ComplaintRegistration");
        }
    }
}
