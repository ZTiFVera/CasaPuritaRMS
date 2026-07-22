using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CasaPuritaRMS.Models
{
    public class MaintenanceRequest
    {
        [Key]
        public int Request_ID { get; set; }

        [Required(ErrorMessage = "Please select a tenant.")]
        [Display(Name = "Tenant")]
        [ForeignKey("Tenant")]
        public int Tenant_ID { get; set; }
        public Tenant? Tenant { get; set; }

        [Required(ErrorMessage = "Title is required.")]
        [StringLength(100)]
        [Display(Name = "Title")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Description is required.")]
        [StringLength(1000)]
        [Display(Name = "Description")]
        public string Description { get; set; } = string.Empty;

        // Optional stored path to an uploaded attachment.
        [StringLength(255)]
        [Display(Name = "Attachment")]
        public string? Attachment_Path { get; set; }

        [Required]
        [StringLength(20)]
        [Display(Name = "Status")]
        public string Status { get; set; } = "Open";

        // Auto-stamped when the request is filed.
        [DataType(DataType.Date)]
        [Display(Name = "Date Submitted")]
        public DateTime Date_Submitted { get; set; } = DateTime.Now;
    }
}