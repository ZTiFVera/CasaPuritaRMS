using System.ComponentModel.DataAnnotations;

namespace CasaPuritaRMS.Models
{
    public class Unit
    {
        [Key]
        public int Unit_ID { get; set; }

        [Required(ErrorMessage = "Unit number is required.")]
        [StringLength(20)]
        [Display(Name = "Unit Number")]
        public string Unit_Number { get; set; } = string.Empty;

        [Required(ErrorMessage = "Status is required.")]
        [StringLength(30)]
        public string Status { get; set; } = "Available";

        [Required(ErrorMessage = "Floor is required.")]
        [Range(1, 100, ErrorMessage = "Floor must be between 1 and 100.")]
        public int Floor { get; set; }

        [Required(ErrorMessage = "Capacity is required.")]
        [Range(1, 20, ErrorMessage = "Capacity must be between 1 and 20.")]
        public int Capacity { get; set; } = 1;



        [Display(Name = "Comfort Room (CR)")]
        public bool Comfort_Room { get; set; } = false;

        // Navigation property
        public ICollection<Tenant>? Tenants { get; set; }
    }
}
