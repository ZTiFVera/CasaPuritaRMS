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

        [Required(ErrorMessage = "Move-in date is required.")]
        [DataType(DataType.Date)]
        [Display(Name = "Move-In Date")]
        public DateTime Move_In_Date { get; set; } = DateTime.Today;

        [Required(ErrorMessage = "Contract term is required.")]
        [Range(1, 60, ErrorMessage = "Contract term must be 1 to 60 months.")]
        [Display(Name = "Contract Term (Months)")]
        public int Contract_Term_Months { get; set; } = 12;

        [Column(TypeName = "decimal(10,2)")]
        [Range(0, 9999999.99, ErrorMessage = "Down payment cannot be negative.")]
        [Display(Name = "Down Payment")]
        public decimal Down_Payment { get; set; } = 0;

        [Required(ErrorMessage = "Occupancy status is required.")]
        [StringLength(20)]
        [Display(Name = "Occupancy Status")]
        public string Occupancy_Status { get; set; } = "Active";

        [Display(Name = "Unit")]
        [ForeignKey("Unit")]
        public int? Unit_ID { get; set; }

        public Unit? Unit { get; set; }

        [NotMapped]
        [Display(Name = "Tenant Name")]
        public string Full_Name => $"{First_Name} {Last_Name}";

        [NotMapped]
        [Display(Name = "Contract End Date")]
        public DateTime Contract_End_Date => Move_In_Date.AddMonths(Contract_Term_Months);

        // First due equals the move-in date; subsequent dues are monthly.
        [NotMapped]
        public DateTime First_Due_Date => Move_In_Date;

        // Upcoming due date; rolls forward monthly past today.
        [NotMapped]
        [Display(Name = "Next Due Date")]
        public DateTime Next_Due_Date
        {
            get
            {
                var today = DateTime.Today;
                var anchor = Move_In_Date;
                while (anchor < today)
                    anchor = anchor.AddMonths(1);
                return anchor;
            }
        }
    }
}