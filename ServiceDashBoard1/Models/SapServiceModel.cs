using Newtonsoft.Json;

namespace ServiceDashBoard1.Models
{
    // 🔹 Base class for common fields
    public class SapServiceModel
    {
        public string CustomerName { get; set; }

        public string Subject { get; set; }

        public string InternalSerialNum { get; set; }
    }

    // 🔹 Used when creating service (POST)
    public class CreateServiceModel : SapServiceModel
    {
        // Inherits CustomerName & Subject
        // Add POST-only fields here if needed later
    }

    // 🔹 Used when viewing full service details (GET)
    public class ViewServiceModel : SapServiceModel
    {
        [JsonProperty("BPeMail")]
        public string BPeMail { get; set; }

      
        public string BPPhone1 { get; set; }

        [JsonProperty("BPShipToAddress")]
        public string BPShipToAddress { get; set; }

        [JsonProperty("ContactPerson")]
        public string ContactPerson { get; set; }

        [JsonProperty("UpdateDate")]
        public DateTime UpdateDate { get; set; }

        // (Optional fallback)
        [JsonProperty("CreationDate")]
        public DateTime CreationDate { get; set; }
    }
}
