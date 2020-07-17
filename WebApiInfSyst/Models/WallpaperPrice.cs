using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApiInfSyst.Models
{
    public class WallpaperPrice
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int BuyWallpaperID { get; set; }
        public int WallpaperID { get; set; }
        [Column(TypeName = "date")]
        public DateTime WallpaperStartDate { get; set; }
        [Column(TypeName = "date")]
        public DateTime WallpaperEndDate { get; set; }
        [Column("WallpaperPrice")]
        public int WallpaperPrice1 { get; set; }
    }
}
