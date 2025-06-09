using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;


namespace ServiceDashBoard1.Models
{
    public class EmployeeAssignComplaint
    {
        [Key]
        public int Id { get; set; }

        // Foreign key to ComplaintRegistration table
        [ForeignKey("Complaint")]
        public int ComplaintRegistrationId { get; set; }

        // Navigation property
        public ComplaintRegistration Complaint { get; set; }

        // Assigned Employee details
        public int? EmployeeIdNo { get; set; }

        public string? EmployeeNames { get; set; }

        public string Description { get; set; } = "";
    }
}
