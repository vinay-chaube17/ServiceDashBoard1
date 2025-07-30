using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ServiceDashBoard1.Models
{
    public class BaseModel
    {

        //public int Id { get; set; }
        [Column(TypeName = "datetime")]

        public DateTime CreatedDate { get; set; } = DateTime.Now;
        [Column(TypeName = "datetime")]

        public DateTime ModifiedDate { get; set; } = DateTime.Now;


    }
}
