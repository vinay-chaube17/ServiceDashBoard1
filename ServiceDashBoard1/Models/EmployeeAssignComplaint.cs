using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;


namespace ServiceDashBoard1.Models
{
    public class EmployeeAssignComplaint
    {
        [Key]
        public int Id { get; set; }

        // Foreign key to ComplaintRegistration table
        [ForeignKey("ComplaintRegistration")]
        public int ComplaintRegistrationId { get; set; }

        // Navigation property
        public ComplaintRegistration Complaint { get; set; }

        // Assigned Employee details
[Column(TypeName = "TEXT")]

        public string? EmployeeIdNo { get; set; }

[Column(TypeName = "TEXT")]

        public string? EmployeeNames { get; set; }

[Column(TypeName = "TEXT")]

        public string Description { get; set; } = "";
    }
}
