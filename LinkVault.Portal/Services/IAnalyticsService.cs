//using LinkVault.Portal.Models;

//namespace LinkVault.Portal.Services
//{
//    public interface IAnalyticsService
//    {
//        Task LogClickAsync(int linkId, string ipAddress, string userAgent, string referrer);
//        Task<int> GetTotalLinksAsync();
//        Task<int> GetTotalClicksTodayAsync();
//        Task<int> GetTotalClicksThisWeekAsync();
//        Task<int> GetTotalClicksThisMonthAsync();
//        Task<IEnumerable<Link>> GetTopPerformingLinksAsync(int count = 10);
//        Task<IEnumerable<ClickLog>> GetRecentActivityAsync(int count = 20);
//        Task<Dictionary<string, int>> GetClicksByDateAsync(int linkId, int days = 30);
//        Task<Dictionary<string, int>> GetClicksByCountryAsync(int linkId);
//        Task<Dictionary<string, int>> GetClicksByReferrerAsync(int linkId);
//    }
//}