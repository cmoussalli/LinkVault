using LinkVault.Portal.Services;

namespace LinkVault.Portal.Services
{
    public class ContentItem
    {
        public string FileName { get; set; } = string.Empty;
        public string FilePath { get; set; } = string.Empty;
        public string ContentType { get; set; } = string.Empty;
        public long FileSize { get; set; }
        public DateTime CreatedAt { get; set; }
    }


    public class ContentService
    {
        private readonly IWebHostEnvironment _environment;
        private readonly string _contentDirectory;
        private readonly HashSet<string> _allowedContentTypes = new()
        {
            "text/html",
            "application/json",
            "image/jpeg",
            "image/jpg",
            "image/png",
            "image/gif",
            "text/plain"
        };

        public ContentService()
        {
            _contentDirectory = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "content");
        }

        public ContentService(IWebHostEnvironment environment)
        {
            _environment = environment;
            _contentDirectory = Path.Combine(_environment.WebRootPath, "content");

            // Ensure content directory exists
            if (!Directory.Exists(_contentDirectory))
            {
                Directory.CreateDirectory(_contentDirectory);
            }
        }

        public Task<IEnumerable<ContentItem>> GetAvailableContentAsync()
        {
            var contentItems = new List<ContentItem>();
            
            if (!Directory.Exists(_contentDirectory))
                return Task.FromResult<IEnumerable<ContentItem>>(contentItems);

            var files = Directory.GetFiles(_contentDirectory);
            
            foreach (var filePath in files)
            {
                var fileInfo = new FileInfo(filePath);
                var contentType = GetContentTypeFromExtension(fileInfo.Extension);
                
                if (IsValidContentType(contentType))
                {
                    contentItems.Add(new ContentItem
                    {
                        FileName = fileInfo.Name,
                        FilePath = Path.GetRelativePath(_contentDirectory, filePath).Replace('\\', '/'),
                        ContentType = contentType,
                        FileSize = fileInfo.Length,
                        CreatedAt = fileInfo.CreationTime
                    });
                }
            }

            return Task.FromResult<IEnumerable<ContentItem>>(contentItems.OrderByDescending(c => c.CreatedAt));
        }

        public Task<ContentItem?> GetContentByPathAsync(string path)
        {
            var fullPath = Path.Combine(_contentDirectory, path);
            
            if (!File.Exists(fullPath))
                return Task.FromResult<ContentItem?>(null);

            var fileInfo = new FileInfo(fullPath);
            var contentType = GetContentTypeFromExtension(fileInfo.Extension);

            var contentItem = new ContentItem
            {
                FileName = fileInfo.Name,
                FilePath = path,
                ContentType = contentType,
                FileSize = fileInfo.Length,
                CreatedAt = fileInfo.CreationTime
            };

            return Task.FromResult<ContentItem?>(contentItem);
        }

        public async Task<string> SaveUploadedContentAsync(Stream fileStream, string fileName, string contentType)
        {
            if (!IsValidContentType(contentType))
                throw new ArgumentException($"Content type '{contentType}' is not allowed");

            // Generate unique filename to avoid conflicts
            var fileExtension = Path.GetExtension(fileName);
            var uniqueFileName = $"{Guid.NewGuid()}{fileExtension}";
            var filePath = Path.Combine(_contentDirectory, uniqueFileName);

            using (var fileStreamOutput = new FileStream(filePath, FileMode.Create))
            {
                await fileStream.CopyToAsync(fileStreamOutput);
            }

            // Return relative path from wwwroot
            return Path.GetRelativePath(_environment.WebRootPath, filePath).Replace('\\', '/');
        }

        public Task<bool> DeleteContentAsync(string path)
        {
            var fullPath = Path.Combine(_environment.WebRootPath, path);
            
            if (!File.Exists(fullPath))
                return Task.FromResult(false);

            try
            {
                File.Delete(fullPath);
                return Task.FromResult(true);
            }
            catch
            {
                return Task.FromResult(false);
            }
        }

        public async Task<string> GetContentAsync(string path)
        {
            var fullPath = Path.Combine(_environment.WebRootPath, path);
            
            if (!File.Exists(fullPath))
                throw new FileNotFoundException($"Content file not found: {path}");

            return await File.ReadAllTextAsync(fullPath);
        }

        public bool IsValidContentType(string contentType)
        {
            return _allowedContentTypes.Contains(contentType.ToLowerInvariant());
        }

        private string GetContentTypeFromExtension(string extension)
        {
            return extension.ToLowerInvariant() switch
            {
                ".html" or ".htm" => "text/html",
                ".json" => "application/json",
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".gif" => "image/gif",
                ".txt" => "text/plain",
                _ => "application/octet-stream"
            };
        }
    }
}