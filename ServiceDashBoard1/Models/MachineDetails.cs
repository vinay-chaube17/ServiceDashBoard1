using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ServiceDashBoard1.Models
{
    [Table("MasterData")] // 
    public class MachineDetails
    {

        [Column("SR NO")]  // Maps to 'SR NO' column in the table
        public int Id { get; set; }


       
        [Column("M_C NO", TypeName = "Text")]
        // Maps to 'M/C NO' column in the table

        public string MachineSerialNumber { get; set; }

        [Column("CUSTOMER NAME", TypeName = "Text")]  // Maps to 'CUSTOMER NAME' column in the table
        public string CompanyName { get; set; }

        [Column("INSTALLATION  DONE BY", TypeName = "Text")]  // Maps to 'INSTALLATION DONE BY' column in the table
        public string ContactPerson { get; set; }

        [Column("EMAIL ID", TypeName = "Text")]  // Maps to 'EMAIL ID' column in the table
        public string Email { get; set; }

        [Column("CONTACT NO", TypeName = "Text")]  // Maps to 'CONTACT NO' column in the table
        public string PhoneNo { get; set; }

        [Column("CUSTOMER_ADDRESS", TypeName = "Text")]  // Maps to 'CUSTOMER_ADDRESS' column in the table
        public string CompanyAddress { get; set; }


    }
}
