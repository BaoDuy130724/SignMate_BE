using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.Configuration;
using SignMate.Application.Interfaces;

namespace SignMate.Infrastructure.ExternalServices;

public class BlobService : IBlobService
{
    private readonly BlobContainerClient? _container;
    // URL gốc public của chính API này để Python tải lại file đã lưu local
    // (khi chưa cấu hình Azure Blob). Mặc định khớp cổng http trong launchSettings.
    private readonly string _localBaseUrl;

    public BlobService(IConfiguration config)
    {
        _localBaseUrl = (config["PublicBaseUrl"] ?? "http://localhost:5184").TrimEnd('/');

        var connectionString = config["AzureBlob:ConnectionString"];
        var containerName = config["AzureBlob:ContainerName"] ?? "signmate-videos";

        if (string.IsNullOrEmpty(connectionString))
        {
            _container = null;
            return;
        }

        var blobServiceClient = new BlobServiceClient(connectionString);
        _container = blobServiceClient.GetBlobContainerClient(containerName);
        _container.CreateIfNotExists(PublicAccessType.Blob);
    }

    public async Task<string> UploadAsync(Stream stream, string fileName, string contentType)
    {
        if (_container == null)
        {
            // Fallback dev/local: chưa cấu hình Azure -> lưu vào wwwroot/uploads và trả URL tĩnh
            // (UseStaticFiles đã bật) để AI service (Python) tải video về mà chấm điểm.
            var safeName = $"{Guid.NewGuid():N}_{Path.GetFileName(fileName)}";
            var uploadsDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
            Directory.CreateDirectory(uploadsDir);

            var fullPath = Path.Combine(uploadsDir, safeName);
            await using (var fs = new FileStream(fullPath, FileMode.Create, FileAccess.Write))
            {
                await stream.CopyToAsync(fs);
            }
            return $"{_localBaseUrl}/uploads/{safeName}";
        }

        var blobName = $"attempts/{Guid.NewGuid():N}_{Path.GetFileName(fileName)}";
        var blobClient = _container.GetBlobClient(blobName);

        await blobClient.UploadAsync(stream, new BlobHttpHeaders { ContentType = contentType });
        return blobClient.Uri.ToString();
    }

    public async Task DeleteAsync(string blobUrl)
    {
        // File lưu local (fallback): xóa trực tiếp trên đĩa.
        if (blobUrl.StartsWith(_localBaseUrl, StringComparison.OrdinalIgnoreCase))
        {
            var name = Path.GetFileName(new Uri(blobUrl).LocalPath);
            var localPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", name);
            if (File.Exists(localPath)) File.Delete(localPath);
            return;
        }

        if (_container == null) return;

        var uri = new Uri(blobUrl);
        var blobName = string.Join("/", uri.Segments.Skip(2));
        var blobClient = _container.GetBlobClient(blobName);
        await blobClient.DeleteIfExistsAsync();
    }
}
