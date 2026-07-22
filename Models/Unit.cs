using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

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

        [Required(ErrorMessage = "Monthly price is required.")]
        [Column(TypeName = "decimal(10,2)")]
        [Range(0, 999999.99, ErrorMessage = "Price must be between 0 and 999,999.99.")]
        [Display(Name = "Monthly Price")]
        public decimal Monthly_Price { get; set; }

        [Display(Name = "Comfort Room (CR)")]
        public bool Comfort_Room { get; set; } = false;

        // Navigation property
        public ICollection<Tenant>? Tenants { get; set; }
    }
}
