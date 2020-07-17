using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApiInfSyst.Models
{
    public class Games
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int GameID { get; set; }
        [Required]
        [StringLength(50)]
        public string GameName { get; set; }
        [Column(TypeName = "image")]
        [Required]
        public byte[] GameCover { get; set; }
        public int? CountGameBay { get; set; }
        [StringLength(50)]
        public string DeveloperName { get; set; }
        public int? Prt { get; set; }
    }
}
