using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
namespace ServiceDashBoard1.Models
{
    public class ServiceModel
    {
        public int Id { get; set; }  // Primary Key
        public int ComplaintId { get; set; }  // ComplaintRegistration ki Primary Key

        [NotMapped] public string TokenNumber { get; set; }
        [NotMapped] public string MachineSerialNo { get; set; }
        [NotMapped] public string CompanyName { get; set; }
        [NotMapped] public string Email { get; set; }
        [NotMapped] public string PhoneNo { get; set; }
        [NotMapped] public string Address { get; set; }
        [NotMapped] public string ContactPerson { get; set; }
        [NotMapped] public string ComplaintDescription { get; set; }
        [NotMapped] public string Status { get; set; }
        [NotMapped] public string? ImageBase64 { get; set; }

        [NotMapped] public string MainProblemText { get; set; }
        [NotMapped] public string SubProblemText { get; set; }
        // ✅ Sirf yeh database me save hoga
        [Required(ErrorMessage = "Remark is required.")]
        public string Remark { get; set; }

        [Required(ErrorMessage = "Final Remark is required if status is Completed.")]
        [DefaultValue("")]
        public String? FinalRemark { get; set; } = "";

        // ✅ NEW FIELDS for tracking who wrote what
        public string? RemarkBy { get; set; }         // Username of person who wrote Remark
        public string? FinalRemarkBy { get; set; }    // Username of person who wrote FinalRemark

        [NotMapped]
        public List<RemarkHistory>? RemarkHistories { get; set; }




    }
}
