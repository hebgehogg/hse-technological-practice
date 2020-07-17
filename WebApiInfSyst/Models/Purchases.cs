using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApiInfSyst.Models
{
    public class Purchases
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int PurchaseID { get; set; }
        public int ClientID { get; set; }
        [Column(TypeName = "date")]
        public DateTime PurchaseDate { get; set; }
        public int PurchaseCount { get; set; }
    }
}
