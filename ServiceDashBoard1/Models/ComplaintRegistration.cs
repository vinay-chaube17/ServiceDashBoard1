    using Microsoft.AspNetCore.Mvc.Rendering;
using ServiceDashBoard1.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ServiceDashBoard1.Models
{
    // Entity Model for Complaint Registration which inherit from BaseModel
    // Represents the database structure for storing complaint details.
    // This model maps directly to the database using EF Core annotations.



    public class ComplaintRegistration : BaseModel
    {
       

        public int Id { get; set; }

        //Below DataType is used when you want to store a string in MYSQL then we have to use this otherwise you dont want to mention column type in SQL Server
[Column(TypeName = "TEXT")]

        public string TokenNumber{ get; set; }

[Column(TypeName = "TEXT")]

        public string MachineSerialNo { get; set; }

[Column(TypeName = "TEXT")]

        public string CompanyName { get; set; }

[Column(TypeName = "TEXT")]

        [Required(ErrorMessage = "Email is required.")]
        [RegularExpression(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$", ErrorMessage = "Invalid email format.")]

        public string Email { get; set; }

        [Phone]
        [Required]
        [RegularExpression(@"^\d{10}$", ErrorMessage = "Phone number must be exactly 10 digits.")]
[Column(TypeName = "TEXT")]

        public string PhoneNo { get; set; }

[Column(TypeName = "TEXT")]

        public string Address { get; set; }

[Column(TypeName = "TEXT")]

        public string ContactPerson { get; set; }

[Column(TypeName = "TEXT")]

        public string ComplaintDescription { get; set; }


        [Column(TypeName = "TEXT")]
        public string? ImageBase64 { get; set; }

        [Display(Name = "AssignTo")]
[Column(TypeName = "TEXT")]

        public string? Role { get; set; }

[Column(TypeName = "TEXT")]

        // ✅ Store multiple selected Main Problems as comma-separated values
        public string SelectedMainProblems { get; set; }



        //[Column(TypeName = "TEXT")]

        //// This property help to show call id no generated from SAP with help of Machineserialno and CompanyName
        //public string ServiceCallNO { get; set; }



        [Column(TypeName = "TEXT")]

        // ✅ Store multiple selected Sub Problems as comma-separated values
        public string SelectedSubProblems { get; set; }


[Column(TypeName = "TEXT")]

        public string Status { get; set; } = "New"; // Default status

[Column(TypeName = "TEXT")]

        public string? CheckedBy { get; set; } = "-";


        [ForeignKey("TokenSequence")] 
        public int TokenId { get; set; }

        public TokenSequence? TokenSequence { get; set; }

[Column(TypeName = "TEXT")]

        public string? EmployeeId1 { get; set; }

[Column(TypeName = "TEXT")]

        public string? EmployeeName1 { get; set; }

        // ⚠️  EmployeeAssignments Relationship (IMPORTANT)
        public virtual ICollection<EmployeeAssignComplaint> EmployeeAssignments { get; set; } = new List<EmployeeAssignComplaint>();


        [NotMapped] // ⚠️ Isko DB me save nahi karna hai, bas UI control ke liye hai
        public bool CanEdit { get; set; }


        [NotMapped]
        public List<SelectListItem> MainProblemList { get; set; } = new List<SelectListItem>();

        [NotMapped]
        public List<SelectListItem> SubProblemList { get; set; } = new List<SelectListItem>();


    }

}
