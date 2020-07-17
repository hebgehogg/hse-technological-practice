using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApiInfSyst.Models
{
    public class GemesPrices
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int BuyGameID { get; set; }
        public int GameID { get; set; }
        [Column(TypeName = "date")]
        public DateTime GameStartDate { get; set; }
        [Column(TypeName = "date")]
        public DateTime GameEndDate { get; set; }
        public int GamePrice { get; set; }
    }
}
