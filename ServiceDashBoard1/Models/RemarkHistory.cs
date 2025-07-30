using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ServiceDashBoard1.Models
{
    public class RemarkHistory
    {

        public int Id { get; set; }

        [ForeignKey("Complaint")]

        public int ComplaintId { get; set; } // Foreign Key to Complaint or ServiceModel

        [Required]
[Column(TypeName = "TEXT")]

        public string RemarkBy { get; set; }

        [Required]
[Column(TypeName = "TEXT")]

        public string RemarkText { get; set; }

        public DateTime RemarkDate { get; set; } = DateTime.Now;  


    }
}
