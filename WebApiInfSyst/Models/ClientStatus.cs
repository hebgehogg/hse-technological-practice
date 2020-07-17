using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApiInfSyst.Models
{
    public class ClientStatus
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int СlienStatusID { get; set; }
        public int ClientID { get; set; }
        [Column(TypeName = "date")]
        public DateTime StatusStart { get; set; }
        [Column(TypeName = "date")]
        public DateTime? StatusEnd { get; set; }
    }
}
