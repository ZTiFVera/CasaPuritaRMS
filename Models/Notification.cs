using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CasaPuritaRMS.Models
{
    public class Notification
    {
        [Key]
        public int Notification_ID { get; set; }

        [ForeignKey("Tenant")]
        [Display(Name = "Tenant")]
        public int? Tenant_ID { get; set; }
        public Tenant? Tenant { get; set; }

        [Required]
        [StringLength(30)]
        [Display(Name = "Type")]
        public string Type { get; set; } = "General";

        [Required]
        [StringLength(255)]
        [Display(Name = "Message")]
        public string Message { get; set; } = string.Empty;

        // Auto-stamped when the notification is created.
        [Display(Name = "Created At")]
        public DateTime Created_At { get; set; } = DateTime.Now;
    }
}