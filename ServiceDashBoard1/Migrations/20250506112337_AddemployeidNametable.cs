using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ServiceDashBoard1.Migrations
{
    /// <inheritdoc />
    public partial class AddemployeidNametable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DraftComplaints");

            migrationBuilder.AddColumn<int>(
                name: "EmployeeId1",
                table: "ComplaintRegistration",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EmployeeName1",
                table: "ComplaintRegistration",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "EmployeeAssignComplaints",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ComplaintRegistrationId = table.Column<int>(type: "int", nullable: false),
                    EmployeeIdNo = table.Column<int>(type: "int", nullable: true),
                    EmployeeNames = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmployeeAssignComplaints", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EmployeeAssignComplaints_ComplaintRegistration_ComplaintRegistrationId",
                        column: x => x.ComplaintRegistrationId,
                        principalTable: "ComplaintRegistration",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeAssignComplaints_ComplaintRegistrationId",
                table: "EmployeeAssignComplaints",
                column: "ComplaintRegistrationId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EmployeeAssignComplaints");

            migrationBuilder.DropColumn(
                name: "EmployeeId1",
                table: "ComplaintRegistration");

            migrationBuilder.DropColumn(
                name: "EmployeeName1",
                table: "ComplaintRegistration");

            migrationBuilder.CreateTable(
                name: "DraftComplaints",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Address = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CompanyName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ComplaintDescription = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ContactPerson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ImageBase64 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Machine = table.Column<int>(type: "int", nullable: false),
                    MachineSerialNo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PhoneNo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Role = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SelectedMainProblems = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SelectedSubProblems = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TokenNumber = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DraftComplaints", x => x.Id);
                });
        }
    }
}
