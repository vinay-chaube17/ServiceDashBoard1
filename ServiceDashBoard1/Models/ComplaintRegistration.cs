using Microsoft.AspNetCore.Mvc.Rendering;
using ServiceDashBoard1.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ServiceDashBoard1.Models
{
    public class ComplaintRegistration : BaseModel
    {
       

        public int Id { get; set; }

       
        public string TokenNumber{ get; set; }
        public string MachineSerialNo { get; set; }
        public string CompanyName { get; set; }

        [Required(ErrorMessage = "Email is required.")]
        [RegularExpression(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$", ErrorMessage = "Invalid email format.")]

        public string Email { get; set; }

        [Phone]
        [Required]
        [RegularExpression(@"^\d{10}$", ErrorMessage = "Phone number must be exactly 10 digits.")]

        public string PhoneNo { get; set; }

        public string Address { get; set; }
        public string ContactPerson { get; set; }
        public string ComplaintDescription { get; set; }
        public string? ImageBase64 { get; set; }

        [Display(Name = "AssignTo")]
        public string Role { get; set; }

        // ✅ Store multiple selected Main Problems as comma-separated values
        public string SelectedMainProblems { get; set; }

        // ✅ Store multiple selected Sub Problems as comma-separated values
        public string SelectedSubProblems { get; set; }

        public string Status { get; set; } = "New"; // Default status

       
        public string? CheckedBy { get; set; } = "-";


        [ForeignKey("TokenSequence")] 
        public int TokenId { get; set; }

        public TokenSequence? TokenSequence { get; set; }

        [NotMapped] // ⚠️ Isko DB me save nahi karna hai, bas UI control ke liye hai
        public bool CanEdit { get; set; }


        [NotMapped]
        public List<SelectListItem> MainProblemList { get; set; } = new List<SelectListItem>();

        [NotMapped]
        public List<SelectListItem> SubProblemList { get; set; } = new List<SelectListItem>();


    }

}
