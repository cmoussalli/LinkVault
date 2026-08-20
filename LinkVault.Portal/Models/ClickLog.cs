using System.ComponentModel.DataAnnotations;

namespace LinkVault.Portal.Models
{
    public class ClickLog
    {
        public int Id { get; set; }
        
        public int LinkId { get; set; }
        
        [StringLength(45)]
        public string IpAddress { get; set; } = string.Empty;
        
        [StringLength(500)]
        public string UserAgent { get; set; } = string.Empty;
        
        [StringLength(500)]
        public string Referrer { get; set; } = string.Empty;
        
        public DateTime ClickedAt { get; set; }
        
        [StringLength(100)]
        public string Country { get; set; } = string.Empty;
        
        [StringLength(100)]
        public string City { get; set; } = string.Empty;
        
        // Navigation property
        public virtual Link Link { get; set; } = null!;
    }
}