using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApiInfSyst.Models
{
    public class Genres
    {

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int GenreID { get; set; }

        [Required]
        [StringLength(50)]
        public string GenreName { get; set; }
    }
}
