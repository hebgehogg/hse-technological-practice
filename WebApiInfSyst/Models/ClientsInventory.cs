using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApiInfSyst.Models
{
    public class ClientsInventory
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int CIID { get; set; }
        public int InventoryID { get; set; }
        public int ClientID { get; set; }
        [Column(TypeName = "date")]
        public DateTime? BuyDate { get; set; }
    }
}
