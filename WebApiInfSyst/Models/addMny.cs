using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApiInfSyst.Models
{
    public class addMny
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int addMnyID { get; set; }
        public int ClientID { get; set; }
        [Column(TypeName = "date")]
        public DateTime addMnyDate { get; set; }
        public int addMnyCount { get; set; }
    }
}
