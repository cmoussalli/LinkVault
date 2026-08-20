using Microsoft.AspNetCore.Mvc;
using LinkVault.Portal.Services;
using LinkVault.Portal.Models;

namespace LinkVault.Portal.Controllers
{
    [ApiController]
    public class RedirectController : ControllerBase
    {
        LinkService linkService = new();
        AnalyticsService analyticsService = new();


        string _contentDirectory;


        [HttpGet("/{shortCode}")]
        public async Task<IActionResult> RedirectToUrl(string shortCode)
        {
            _contentDirectory = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "content");
            // Validate short code format
            if (string.IsNullOrEmpty(shortCode) || shortCode.Length > 50)
            {
                return NotFound();
            }

            // Get the link
            var link = await linkService.GetLinkByShortCodeAsync(shortCode);
            if (link == null)
            {
                return NotFound();
            }

            // Check if link is expired
            if (link.ExpiresAt.HasValue && link.ExpiresAt.Value < DateTime.UtcNow)
            {
                return NotFound();
            }

            // Get client information
            var ipAddress = GetClientIpAddress();
            var userAgent = Request.Headers.UserAgent.ToString();
            var referrer = Request.Headers.Referer.ToString();

            // Log the click asynchronously
            await analyticsService.LogClickAsync(link.Id, ipAddress, userAgent, referrer);
            await linkService.IncrementClickCountAsync(link.Id);

            // Handle based on link type
            if (link.Type == LinkType.Redirect)
            {
                // Redirect to the original URL
                return Redirect(link.RedirectUrl);
            }
            else if (link.Type == LinkType.Content)
            {
                // Serve content
                return await ServeContent(link);
            }

            return NotFound();
        }

        private async Task<IActionResult> ServeContent(Link link)
        {
            if (string.IsNullOrEmpty(link.ContentPath))
            {
                return NotFound();
            }

            var fullPath = Path.Combine(_contentDirectory, link.ContentPath);
            
            if (!System.IO.File.Exists(fullPath))
            {
                return NotFound();
            }

            var contentType = link.ContentType ?? GetContentTypeFromPath(fullPath);
            
            // For HTML content, read and return as HTML
            if (contentType == "text/html")
            {
                var htmlContent = await System.IO.File.ReadAllTextAsync(fullPath);
                return Content(htmlContent, "text/html");
            }
            
            // For JSON content, return as JSON
            if (contentType == "application/json")
            {
                var jsonContent = await System.IO.File.ReadAllTextAsync(fullPath);
                return Content(jsonContent, "application/json");
            }
            
            // For images and other files, return as file
            var fileBytes = await System.IO.File.ReadAllBytesAsync(fullPath);
            return File(fileBytes, contentType);
        }

        private string GetContentTypeFromPath(string filePath)
        {
            var extension = Path.GetExtension(filePath).ToLowerInvariant();
            return extension switch
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

        private string GetClientIpAddress()
        {
            // Check for forwarded IP first (in case of proxy/load balancer)
            var forwardedFor = Request.Headers["X-Forwarded-For"].FirstOrDefault();
            if (!string.IsNullOrEmpty(forwardedFor))
            {
                return forwardedFor.Split(',')[0].Trim();
            }

            // Check for real IP header
            var realIp = Request.Headers["X-Real-IP"].FirstOrDefault();
            if (!string.IsNullOrEmpty(realIp))
            {
                return realIp;
            }

            // Fall back to connection remote IP
            return HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
        }
    }
}