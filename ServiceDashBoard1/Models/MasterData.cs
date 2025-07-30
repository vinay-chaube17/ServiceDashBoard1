using System.ComponentModel.DataAnnotations.Schema;

namespace ServiceDashBoard1.Models
{
   

    public class MasterData
    {
        public int Id { get; set; }
        public string CompanyName { get; set; }
        // public string CompanyAddress { get; set; }
        public string MachineSerialNumber { get; set; }
        public string LaserPower { get; set; }
        public string Email { get; set; }
        public string AddressLine1 { get; set; }
        public string AddressLine2 { get; set; }
        public string? District { get; set; }
        public string State { get; set; }
        public string Pincode { get; set; }
        public string Country { get; set; }

        public string CompanyAddress
        {
            get
            {
                var addressParts = new[]
                {
                AddressLine1,
                AddressLine2,
                District,
                State,
                Pincode,
                Country
            };

                // Join non-empty parts with a comma separator
                return string.Join(", ", addressParts.Where(part => !string.IsNullOrWhiteSpace(part)));
            }
        }

    }
}
