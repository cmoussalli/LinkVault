using Microsoft.EntityFrameworkCore;
using LinkVault.Portal.Data;
using LinkVault.Portal.Models;

namespace LinkVault.Portal.Services
{
    public class LinkService
    {
        private readonly LinkVaultDbContext _context;
        private readonly Random _random = new();

        public LinkService()
        {
            _context = new LinkVaultDbContext();
        }

        public async Task<IEnumerable<Link>> GetAllLinksAsync()
        {
            return await _context.Links
                .OrderByDescending(l => l.CreatedAt)
                .ToListAsync();
        }

        public async Task<Link?> GetLinkByIdAsync(int id)
        {
            return await _context.Links
                .Include(l => l.ClickLogs)
                .FirstOrDefaultAsync(l => l.Id == id);
        }

        public async Task<Link?> GetLinkByShortCodeAsync(string shortCode)
        {
            return await _context.Links
                .FirstOrDefaultAsync(l => l.ShortCode == shortCode && l.IsActive);
        }

        public async Task<Link> CreateLinkAsync(Link link)
        {
            if (string.IsNullOrEmpty(link.ShortCode))
            {
                link.ShortCode = await GenerateUniqueShortCodeAsync();
            }

            link.CreatedAt = DateTime.UtcNow;
            _context.Links.Add(link);
            await _context.SaveChangesAsync();
            return link;
        }

        public async Task<Link> UpdateLinkAsync(Link link)
        {
            _context.Links.Update(link);
            await _context.SaveChangesAsync();
            return link;
        }

        public async Task DeleteLinkAsync(int id)
        {
            var link = await _context.Links.FindAsync(id);
            if (link != null)
            {
                _context.Links.Remove(link);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<string> GenerateUniqueShortCodeAsync()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            string shortCode;
            
            do
            {
                shortCode = new string(Enumerable.Repeat(chars, 6)
                    .Select(s => s[_random.Next(s.Length)]).ToArray());
            }
            while (!await IsShortCodeUniqueAsync(shortCode));

            return shortCode;
        }

        public async Task<bool> IsShortCodeUniqueAsync(string shortCode)
        {
            return !await _context.Links.AnyAsync(l => l.ShortCode == shortCode);
        }

        public async Task IncrementClickCountAsync(int linkId)
        {
            var link = await _context.Links.FindAsync(linkId);
            if (link != null)
            {
                link.ClickCount++;
                await _context.SaveChangesAsync();
            }
        }
    }
}