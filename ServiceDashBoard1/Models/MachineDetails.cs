using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ServiceDashBoard1.Models
{
    public class MachineDetails
    {

        [Column("SR NO")]  // Maps to 'SR NO' column in the table
        public int Id { get; set; }
        [Key]
        [Column("M/C NO")]  // Maps to 'M/C NO' column in the table
        public string MachineSerialNo { get; set; }

        [Column("CUSTOMER NAME")]  // Maps to 'CUSTOMER NAME' column in the table
        public string CompanyName { get; set; }

        [Column("INSTALLATION  DONE BY")]  // Maps to 'INSTALLATION DONE BY' column in the table
        public string ContactPerson { get; set; }

        [Column("EMAIL ID")]  // Maps to 'EMAIL ID' column in the table
        public string Email { get; set; }

        [Column("CONTACT NO")]  // Maps to 'CONTACT NO' column in the table
        public string PhoneNo { get; set; }

        [Column("CUSTOMER_ADDRESS")]  // Maps to 'CUSTOMER_ADDRESS' column in the table
        public string Address { get; set; }


    }
}
