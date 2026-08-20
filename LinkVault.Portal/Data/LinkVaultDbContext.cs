using Microsoft.EntityFrameworkCore;
using LinkVault.Portal.Models;
using CMouss.IdentityFramework;

namespace LinkVault.Portal.Data
{
    public class LinkVaultDbContext : IDFDBContext
    {

        public DbSet<Link> Links { get; set; }
        public DbSet<ClickLog> ClickLogs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure Link entity
            modelBuilder.Entity<Link>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.ShortCode).IsUnique();
                entity.Property(e => e.ShortCode).IsRequired().HasMaxLength(50);
                entity.Property(e => e.RedirectUrl).HasMaxLength(2000);
                entity.Property(e => e.Title).HasMaxLength(200);
                entity.Property(e => e.Type).HasDefaultValue(LinkType.Redirect);
                entity.Property(e => e.ContentPath).HasMaxLength(500);
                entity.Property(e => e.ContentType).HasMaxLength(50);
            });

            // Configure ClickLog entity
            modelBuilder.Entity<ClickLog>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.IpAddress).HasMaxLength(45);
                entity.Property(e => e.UserAgent).HasMaxLength(500);
                entity.Property(e => e.Referrer).HasMaxLength(500);
                entity.Property(e => e.Country).HasMaxLength(100);
                entity.Property(e => e.City).HasMaxLength(100);

                // Configure relationship
                entity.HasOne(e => e.Link)
                      .WithMany(l => l.ClickLogs)
                      .HasForeignKey(e => e.LinkId)
                      .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}