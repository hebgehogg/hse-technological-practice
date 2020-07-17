using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApiInfSyst.Models
{
    public class GameGenre
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int GGID { get; set; }
        public int GameID { get; set; }
        public int GenreID { get; set; }
    }
}
