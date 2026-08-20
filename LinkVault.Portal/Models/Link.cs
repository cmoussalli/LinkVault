using System.ComponentModel.DataAnnotations;

namespace LinkVault.Portal.Models
{
    public enum LinkType
    {
        Redirect = 0,
        Content = 1
    }

    public class Link
    {
        public int Id { get; set; }
        
        [Required]
        [StringLength(50)]
        public string ShortCode { get; set; } = string.Empty;
        
        [StringLength(2000)]
        public string? RedirectUrl { get; set; } = string.Empty;
        
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;
        
        public LinkType Type { get; set; } = LinkType.Redirect;
        
        [StringLength(500)]
        public string? ContentPath { get; set; }
        
        [StringLength(50)]
        public string? ContentType { get; set; }
        
        public DateTime CreatedAt { get; set; }
        
        public DateTime? ExpiresAt { get; set; }
        
        public bool IsActive { get; set; } = true;
        
        public int ClickCount { get; set; } = 0;
        
        // Navigation property
        public virtual ICollection<ClickLog> ClickLogs { get; set; } = new List<ClickLog>();
    }
}