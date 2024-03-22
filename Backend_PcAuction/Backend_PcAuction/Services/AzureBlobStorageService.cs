using Azure.Storage.Blobs;
using Azure.Storage.Sas;

namespace Backend_PcAuction.Services
{
    public interface IAzureBlobStorageService
    {
        Task<string> UploadImageAsync(IFormFile? image);
        Task DeleteImageAsync(string uri);
    }
    public class AzureBlobStorageService : IAzureBlobStorageService
    {
        private readonly BlobContainerClient _blobContainerClient;

        public AzureBlobStorageService(BlobContainerClient blobContainerClient)
        {
            _blobContainerClient = blobContainerClient;
        }

        public async Task<string> UploadImageAsync(IFormFile? imageFile)
        {
            if (imageFile == null || imageFile.Length == 0 || !imageFile.ContentType.StartsWith("image", StringComparison.OrdinalIgnoreCase))
                return "/default.jpg";

            var imageBlobName = string.Format("{0}{1}", Guid.NewGuid(), Path.GetExtension(imageFile.FileName));

            try
            {
                var blobClient = _blobContainerClient.GetBlobClient(imageBlobName);

                using (var stream = imageFile.OpenReadStream())
                {
                    await blobClient.UploadAsync(stream);

                    var permissions = BlobContainerSasPermissions.Read;
                    var expiresOn = DateTimeOffset.MaxValue;
                    var sasUri = _blobContainerClient.GenerateSasUri(permissions, expiresOn);
                    var fullUriWithSas = new UriBuilder(sasUri)
                    {
                        Scheme = "https",
                        Path = $"{_blobContainerClient.Name}/{imageBlobName}"
                    }.Uri;

                    return fullUriWithSas.ToString();
                }
            }
            catch
            {
                return "/default.jpg";
            }
        }

        public async Task DeleteImageAsync(string uri)
        {
            try
            {
                var blobWithSasName = uri.Substring(uri.LastIndexOf('/') + 1);
                var imageBlobName = blobWithSasName.Split('?')[0];
                await _blobContainerClient.DeleteBlobAsync(imageBlobName);
            }
            catch { }
        }
    }
}
