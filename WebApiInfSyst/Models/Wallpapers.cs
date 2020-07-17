using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApiInfSyst.Models
{
    public class Wallpapers
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int WallpaperID { get; set; }
        [Column(TypeName = "image")]
        [Required]
        public byte[] WallpaperPhoto { get; set; }
        [Required]
        [StringLength(50)]
        public string WallpaperName { get; set; }
    }
}
