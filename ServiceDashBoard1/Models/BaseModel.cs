using System.ComponentModel.DataAnnotations;

namespace ServiceDashBoard1.Models
{
    public class BaseModel
    {
       
        //public int Id { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public DateTime ModifiedDate { get; set; } = DateTime.Now;


    }
}
