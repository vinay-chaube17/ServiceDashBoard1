using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ServiceDashBoard1.Migrations
{
    /// <inheritdoc />
    public partial class AdddraftComplaint : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Role",
                table: "ComplaintRegistration",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.CreateTable(
                name: "DraftComplaints",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MachineSerialNo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Machine = table.Column<int>(type: "int", nullable: false),
                    TokenNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CompanyName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PhoneNo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Address = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ContactPerson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ComplaintDescription = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ImageBase64 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Role = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SelectedMainProblems = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SelectedSubProblems = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DraftComplaints", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MachineDetails",
                columns: table => new
                {
                    MCNO = table.Column<string>(name: "M/C NO", type: "nvarchar(450)", nullable: false),
                    SRNO = table.Column<int>(name: "SR NO", type: "int", nullable: false),
                    CUSTOMERNAME = table.Column<string>(name: "CUSTOMER NAME", type: "nvarchar(max)", nullable: false),
                    INSTALLATIONDONEBY = table.Column<string>(name: "INSTALLATION  DONE BY", type: "nvarchar(max)", nullable: false),
                    EMAILID = table.Column<string>(name: "EMAIL ID", type: "nvarchar(max)", nullable: false),
                    CONTACTNO = table.Column<string>(name: "CONTACT NO", type: "nvarchar(max)", nullable: false),
                    CUSTOMER_ADDRESS = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MachineDetails", x => x.MCNO);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DraftComplaints");

            //migrationBuilder.DropTable(
            //    name: "MachineDetails");

            migrationBuilder.AlterColumn<string>(
                name: "Role",
                table: "ComplaintRegistration",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);
        }
    }
}
