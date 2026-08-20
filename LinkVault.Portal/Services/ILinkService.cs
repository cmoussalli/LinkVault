//using LinkVault.Portal.Models;

//namespace LinkVault.Portal.Services
//{
//    public interface ILinkService
//    {
//        Task<IEnumerable<Link>> GetAllLinksAsync();
//        Task<Link?> GetLinkByIdAsync(int id);
//        Task<Link?> GetLinkByShortCodeAsync(string shortCode);
//        Task<Link> CreateLinkAsync(Link link);
//        Task<Link> UpdateLinkAsync(Link link);
//        Task DeleteLinkAsync(int id);
//        Task<string> GenerateUniqueShortCodeAsync();
//        Task<bool> IsShortCodeUniqueAsync(string shortCode);
//        Task IncrementClickCountAsync(int linkId);
//    }
//}