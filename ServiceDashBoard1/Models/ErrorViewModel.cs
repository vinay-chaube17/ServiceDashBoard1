using System.ComponentModel.DataAnnotations.Schema;

namespace ServiceDashBoard1.Models
{
    public class ErrorViewModel
    {
[Column(TypeName = "TEXT")]

        public string? RequestId { get; set; }

        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
    }
}
