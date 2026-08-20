//using LinkVault.Portal.Models;

//namespace LinkVault.Portal.Services
//{
//    public class ContentItem
//    {
//        public string FileName { get; set; } = string.Empty;
//        public string FilePath { get; set; } = string.Empty;
//        public string ContentType { get; set; } = string.Empty;
//        public long FileSize { get; set; }
//        public DateTime CreatedAt { get; set; }
//    }

//    public interface IContentService
//    {
//        Task<IEnumerable<ContentItem>> GetAvailableContentAsync();
//        Task<ContentItem?> GetContentByPathAsync(string path);
//        Task<string> SaveUploadedContentAsync(Stream fileStream, string fileName, string contentType);
//        Task<bool> DeleteContentAsync(string path);
//        Task<string> GetContentAsync(string path);
//        bool IsValidContentType(string contentType);
//    }
//}