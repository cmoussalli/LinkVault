using Microsoft.EntityFrameworkCore;
using LinkVault.Portal.Data;
using LinkVault.Portal.Models;

namespace LinkVault.Portal.Services
{
    public class AnalyticsService
    {
        private readonly LinkVaultDbContext _context;

        public AnalyticsService()
        {
            _context = new();
        }

        public async Task LogClickAsync(int linkId, string ipAddress, string userAgent, string referrer)
        {
            var clickLog = new ClickLog
            {
                LinkId = linkId,
                IpAddress = ipAddress,
                UserAgent = userAgent,
                Referrer = referrer,
                ClickedAt = DateTime.UtcNow,
                Country = "Unknown", // Could be enhanced with IP geolocation service
                City = "Unknown"
            };

            _context.ClickLogs.Add(clickLog);
            await _context.SaveChangesAsync();
        }

        public async Task<int> GetTotalLinksAsync()
        {
            return await _context.Links.CountAsync();
        }

        public async Task<int> GetTotalClicksTodayAsync()
        {
            var today = DateTime.UtcNow.Date;
            return await _context.ClickLogs
                .CountAsync(c => c.ClickedAt >= today);
        }

        public async Task<int> GetTotalClicksThisWeekAsync()
        {
            var weekStart = DateTime.UtcNow.Date.AddDays(-(int)DateTime.UtcNow.DayOfWeek);
            return await _context.ClickLogs
                .CountAsync(c => c.ClickedAt >= weekStart);
        }

        public async Task<int> GetTotalClicksThisMonthAsync()
        {
            var monthStart = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);
            return await _context.ClickLogs
                .CountAsync(c => c.ClickedAt >= monthStart);
        }

        public async Task<IEnumerable<Link>> GetTopPerformingLinksAsync(int count = 10)
        {
            return await _context.Links
                .OrderByDescending(l => l.ClickCount)
                .Take(count)
                .ToListAsync();
        }

        public async Task<IEnumerable<ClickLog>> GetRecentActivityAsync(int count = 20)
        {
            return await _context.ClickLogs
                .Include(c => c.Link)
                .OrderByDescending(c => c.ClickedAt)
                .Take(count)
                .ToListAsync();
        }

        public async Task<Dictionary<string, int>> GetClicksByDateAsync(int linkId, int days = 30)
        {
            var startDate = DateTime.UtcNow.Date.AddDays(-days);
            
            var clicks = await _context.ClickLogs
                .Where(c => c.LinkId == linkId && c.ClickedAt >= startDate)
                .GroupBy(c => c.ClickedAt.Date)
                .Select(g => new { Date = g.Key, Count = g.Count() })
                .OrderBy(x => x.Date)
                .ToListAsync();

            return clicks.ToDictionary(
                x => x.Date.ToString("yyyy-MM-dd"),
                x => x.Count
            );
        }

        public async Task<Dictionary<string, int>> GetClicksByCountryAsync(int linkId)
        {
            var clicks = await _context.ClickLogs
                .Where(c => c.LinkId == linkId)
                .GroupBy(c => c.Country)
                .Select(g => new { Country = g.Key, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .ToListAsync();

            return clicks.ToDictionary(x => x.Country, x => x.Count);
        }

        public async Task<Dictionary<string, int>> GetClicksByReferrerAsync(int linkId)
        {
            var clicks = await _context.ClickLogs
                .Where(c => c.LinkId == linkId)
                .GroupBy(c => string.IsNullOrEmpty(c.Referrer) ? "Direct" : c.Referrer)
                .Select(g => new { Referrer = g.Key, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .Take(10)
                .ToListAsync();

            return clicks.ToDictionary(x => x.Referrer, x => x.Count);
        }
    }
}