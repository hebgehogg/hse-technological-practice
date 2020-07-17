using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApiInfSyst.Models
{
    public class Staff
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int StaffID { get; set; }
        [Required]
        [StringLength(50)]
        public string StaffName { get; set; }
        [Required]
        [StringLength(50)]
        public string StaffLogin { get; set; }
        [Required]
        [StringLength(256)]
        public string StaffPasword { get; set; }
        [Required]
        [StringLength(1)]
        public string StaffType { get; set; }
    }
}
