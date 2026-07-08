using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CasaPuritaRMS.Models
{
    public class Tenant
    {
        [Key]
        public int Tenant_ID { get; set; }

        [Required(ErrorMessage = "First name is required.")]
        [StringLength(50)]
        [Display(Name = "First Name")]
        public string First_Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Last name is required.")]
        [StringLength(50)]
        [Display(Name = "Last Name")]
        public string Last_Name { get; set; } = string.Empty;

        [EmailAddress(ErrorMessage = "Invalid email format.")]
        [StringLength(100)]
        public string? Email { get; set; }

        [Required(ErrorMessage = "Address is required.")]
        [StringLength(255)]
        [Display(Name = "Address")]
        public string Address { get; set; } = string.Empty;

        [Required(ErrorMessage = "Contact number is required.")]
        [StringLength(20)]
        [Display(Name = "Contact Number")]
        public string Contact_Number { get; set; } = string.Empty;

        [StringLength(100)]
        [Display(Name = "Emergency Contact Name")]
        public string? Emergency_Contact_Name { get; set; }

        [StringLength(20)]
        [Display(Name = "Emergency Contact Number")]
        public string? Emergency_Contact_Number { get; set; }



        [Required(ErrorMessage = "Occupancy status is required.")]
        [StringLength(20)]
        [Display(Name = "Occupancy Status")]
        public string Occupancy_Status { get; set; } = "Active";


        [Display(Name = "Unit")]
        [ForeignKey("Unit")]
        public int? Unit_ID { get; set; }   

        // Navigation property
        public Unit? Unit { get; set; }     
    }

}
