using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
namespace ServiceDashBoard1.Models
{
    public class ServiceModel
    {
        public int Id { get; set; }  // Primary Key
        public int ComplaintId { get; set; }  // ComplaintRegistration model Primary Key but here its acts as a foreign key 

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

        // below property only saved in ServiceModel database 
        [Required(ErrorMessage = "Remark is required.")]
[Column(TypeName = "TEXT")]

        public string Remark { get; set; }

        [Required(ErrorMessage = "Final Remark is required if status is Completed.")]
        [DefaultValue("")]
[Column(TypeName = "TEXT")]

        public String? FinalRemark { get; set; } = "";

        // ✅ NEW FIELDS for tracking who wrote what
[Column(TypeName = "TEXT")]

        public string? RemarkBy { get; set; }

        // Username of person who wrote Remark
[Column(TypeName = "TEXT")]

        public string? FinalRemarkBy { get; set; }    // Username of person who wrote FinalRemark

        [NotMapped]
        public List<RemarkHistory>? RemarkHistories { get; set; }

       



    }
}
