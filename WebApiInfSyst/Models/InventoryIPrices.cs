using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApiInfSyst.Models
{
    public class InventoryIPrices
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int BayInventoryID { get; set; }
        public int InventoryID { get; set; }
        [Column(TypeName = "date")]
        public DateTime InventoryStartDate { get; set; }
        [Column(TypeName = "date")]
        public DateTime InventoryEndDate { get; set; }
        public int InventoryIPrice { get; set; }
    }
}
