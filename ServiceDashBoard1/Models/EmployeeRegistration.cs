using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ServiceDashBoard1.Models
{
    public class EmployeeRegistration
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Company Name is required")]
[Column(TypeName = "TEXT")]

        public string CompanyName { get; set; }

        [Required(ErrorMessage = "Company Address is required")]
[Column(TypeName = "TEXT")]


        public string CompanyAddress { get; set; }

        [Required(ErrorMessage = "Pincode is required")]
        [RegularExpression(@"^\d{6}$", ErrorMessage = "Pincode must be a 6-digit number")]
[Column(TypeName = "TEXT")]

        public string Pincode { get; set; }

        [Required(ErrorMessage = "Contact Person Name is required")]
[Column(TypeName = "TEXT")]


        public string ContactPersonName { get; set; }

        [Required(ErrorMessage = "PhoneNo is required.")]
        [RegularExpression(@"^\d{10}$", ErrorMessage = "Phone number must be exactly 10 digits.")]
[Column(TypeName = "TEXT")]

        public string PhoneNo { get; set; }

        [Required(ErrorMessage = "Email is required.")]
        [RegularExpression(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$", ErrorMessage = "Invalid email format.")]
[Column(TypeName = "TEXT")]

        public string Email { get; set; }


        [Required(ErrorMessage = "Username is required.")]
[Column(TypeName = "TEXT")]

        public string Username { get; set; }

        [Required(ErrorMessage = "Password is required.")]
[Column(TypeName = "TEXT")]

        public string Password { get; set; }


        [Required(ErrorMessage = "Machine Serial Number is required")]
[Column(TypeName = "TEXT")]

        public string MachineSerialNo { get; set; }

        [Required(ErrorMessage = "Machine Serial Number is required")]
[Column(TypeName = "TEXT")]


        public string MachineType { get; set; }

        [Required(ErrorMessage = "You have to select.")]
[Column(TypeName = "TEXT")]

        public string? isActive { get; set; }


[Column(TypeName = "TEXT")]

        public string? Role { get; set; }

        // ✅ Ignore ModifiedDate only for this entity
        public DateTime? CreatedDate { get; set; } = DateTime.Now;

    }
}
