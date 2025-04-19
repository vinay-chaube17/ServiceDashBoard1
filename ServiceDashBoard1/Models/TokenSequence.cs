
using System.ComponentModel.DataAnnotations;

namespace ServiceDashBoard1.Models
{
    public class TokenSequence
    {
        [Key]
        public int Id { get; set; } // ✅ ID always = 1
        public int NextTokenNumber { get; set; } // 🔥 Safe Counter

        
    }
}
