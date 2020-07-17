using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApiInfSyst.Models
{
    public class Client
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int ClientID { get; set; }

        [Required]
        [StringLength(50)]
        public string ClientName { get; set; }

        [Required]
        [StringLength(50)]
        public string ClientLogin { get; set; }

        [Required]
        [StringLength(256)]
        public string ClientPassword { get; set; }

        [Column(TypeName = "date")]
        public DateTime ClientRegistrationDate { get; set; }

        [Required]
        [StringLength(7)]
        public string ClientStatus { get; set; }

        public int? WallpaperID { get; set; }

        public int? cash { get; set; }
    }
}
