using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApiInfSyst.Models
{
    public class ClientsGames
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int CGID { get; set; }
        public int ClientID { get; set; }
        public int GameID { get; set; }
        [Column(TypeName = "date")]
        public DateTime GamePurchaseDate { get; set; }
    }
}
