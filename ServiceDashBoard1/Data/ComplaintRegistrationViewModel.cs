

using Microsoft.AspNetCore.Mvc.Rendering;
using ServiceDashBoard1.Enums;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using System.ComponentModel;

namespace ServiceDashBoard1.Data
{
    public class ComplaintRegistrationViewModel
    {
        public int Id { get; set; }

        public string TokenNumber { get; set; }

        [Required]
        public string MachineSerialNo { get; set; }

        [Required]
        public string CompanyName { get; set; }

        [Required(ErrorMessage = "Email is required.")]
        [RegularExpression(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$", ErrorMessage = "Invalid email format.")]

        public string Email { get; set; }

        [Phone]
        [Required]
        [RegularExpression(@"^\d{10}$", ErrorMessage = "Phone number must be exactly 10 digits.")]

        public string PhoneNo { get; set; }

        [Required]
        public string Address { get; set; }

        [Required]
        public string ContactPerson { get; set; }

        [Required]
        public string ComplaintDescription { get; set; }

        public string? ImageBase64 { get; set; }

        [Required]
        [Display(Name = "AssignRole")] 
        public string Role { get; set; }

        // ✅ Multiple Main Problems ko support karne ke liye List<string>
        [Required]
        public List<string> SelectedMainProblems { get; set; } = new List<string>();

        // ✅ Multiple Sub Problems ko support karne ke liye List<string>
        [Required]
        public List<string> SelectedSubProblems { get; set; } = new List<string>();

        public string Status { get; set; }

        // ✅ Dropdown ke liye SelectListItem lists
        public List<SelectListItem> MainProblemList { get; set; } = new List<SelectListItem>();
        public List<SelectListItem> SubProblemList { get; set; } = new List<SelectListItem>();
    }
}
