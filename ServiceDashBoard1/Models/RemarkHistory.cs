using System.ComponentModel.DataAnnotations;

namespace ServiceDashBoard1.Models
{
    public class RemarkHistory
    {

        public int Id { get; set; }

        public int ComplaintId { get; set; } // Foreign Key to Complaint or ServiceModel

        [Required]
        public string RemarkBy { get; set; }

        [Required]
        public string RemarkText { get; set; }

        public DateTime RemarkDate { get; set; } = DateTime.Now;  


    }
}
