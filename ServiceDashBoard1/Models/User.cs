﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ServiceDashBoard1.Models
{
    public class User
    {

        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Name is required.")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Email is required.")]
        [RegularExpression(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$", ErrorMessage = "Invalid email format.")]

        public string EmailId { get; set; }

        [Required(ErrorMessage = "PhoneNo is required.")]
        [RegularExpression(@"^\d{10}$", ErrorMessage = "Phone number must be exactly 10 digits.")]
        public long  PhoneNo { get; set; }

        public  string EmployeeId { get; set; }

        [Required(ErrorMessage = "Address is required.")]
        public string Address { get; set; }

        [Required(ErrorMessage = "Username is required.")]
        public string Username { get; set; }

        [Required(ErrorMessage = "Password is required.")]
        public string Password { get; set; }
        public string Role { get; set; }

        [Required(ErrorMessage ="You have to select.")]
        public string? isActive { get; set; }

        // ✅ New property to track first-time login
        public bool IsFirstLogin { get; set; } = true;

        public bool ShowPasswordChangePopup { get; set; } = true;

        // ✅ Ignore ModifiedDate only for this entity
        public DateTime? CreatedDate { get; set; } = DateTime.Now;



    }
}
