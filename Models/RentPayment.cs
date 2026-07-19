using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CasaPuritaRMS.Models
{
    public class RentPayment
    {
        [Key]
        public int Payment_ID { get; set; }

        [Required(ErrorMessage = "Please select a tenant.")]
        [Display(Name = "Tenant")]
        [ForeignKey("Tenant")]
        public int Tenant_ID { get; set; }
        public Tenant? Tenant { get; set; }

        [Required(ErrorMessage = "Monthly rent is required.")]
        [Column(TypeName = "decimal(10,2)")]
        [Display(Name = "Monthly Rent")]
        public decimal Monthly_Rent { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        [Display(Name = "Down Payment")]
        public decimal Down_Payment { get; set; } = 0;

        [Required(ErrorMessage = "Amount paid is required.")]
        [Column(TypeName = "decimal(10,2)")]
        [Display(Name = "Amount Paid")]
        public decimal Amount_Paid { get; set; }

        [Required(ErrorMessage = "Payment date is required.")]
        [DataType(DataType.Date)]
        [Display(Name = "Payment Date")]
        public DateTime Payment_Date { get; set; } = DateTime.Now;

        [Required(ErrorMessage = "Due date is required.")]
        [DataType(DataType.Date)]
        [Display(Name = "Due Date")]
        public DateTime Due_Date { get; set; }


        [Required]
        [StringLength(30)]
        [Display(Name = "Payment Status")]
        public string Payment_Status { get; set; } = "Pending";

        [StringLength(30)]
        [Display(Name = "Receipt Number")]
        public string? Receipt_Number { get; set; }

      

        [NotMapped]
        [Display(Name = "Total Paid")]
        public decimal Total_Paid => Down_Payment + Amount_Paid;

        [NotMapped]
        [Display(Name = "Balance")]
        public decimal Balance => Monthly_Rent > Total_Paid ? Monthly_Rent - Total_Paid : 0;

        [NotMapped]
        [Display(Name = "Advance / Overpayment")]
        public decimal Advance_Credit => Total_Paid > Monthly_Rent ? Total_Paid - Monthly_Rent : 0;
    }
}