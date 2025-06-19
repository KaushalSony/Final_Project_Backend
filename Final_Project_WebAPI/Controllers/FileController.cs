using Final_Project_WebAPI.Services;
using Microsoft.AspNetCore.Mvc;
using Azure.Storage.Blobs;
using Microsoft.AspNetCore.Authorization;
using System.Net.Http.Headers;

namespace Final_Project_WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class FileController : ControllerBase
    {
        private readonly BlobStorageService _blobStorageService;
        private readonly ILogger<FileController> _logger;
        private readonly BlobServiceClient _blobServiceClient;
        private readonly string _containerName;

        public FileController(BlobStorageService blobStorageService,ILogger<FileController> logger,IConfiguration configuration)
        {
            _blobStorageService = blobStorageService;
            _logger = logger;
            var connectionString = configuration.GetSection("BlobStorage:ConnectionString").Value;
            _containerName = configuration.GetSection("BlobStorage:ContainerName").Value;
            _blobServiceClient = new BlobServiceClient(connectionString);
        }

        // GET: api/File/verify-container
        [HttpGet("verify-container")]
        public async Task<IActionResult> VerifyContainer()
        {
            try
            {
                var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
                var exists = await containerClient.ExistsAsync();

                if (!exists)
                {
                    await containerClient.CreateAsync();
                    return Ok(new { message = "Container created successfully", containerName = _containerName });
                }

                var blobs = new List<string>();
                await foreach (var blob in containerClient.GetBlobsAsync())
                {
                    blobs.Add(blob.Name);
                }

                return Ok(new
                {
                    message = "Container exists",
                    containerName = _containerName,
                    blobCount = blobs.Count,
                    blobs = blobs
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verifying container");
                return StatusCode(500, new { error = "Error verifying container", details = ex.Message });
            }
        }

        // POST: api/File/upload
        [HttpPost("upload")]
        public async Task<IActionResult> UploadFile(IFormFile file)
        {
            try
            {
                if (file == null || file.Length == 0)
                    return BadRequest("No file uploaded");

                _logger.LogInformation($"Starting file upload. File name: {file.FileName}, Size: {file.Length} bytes");

                var fileName = $"{Guid.NewGuid()}_{ContentDispositionHeaderValue.Parse(file.ContentDisposition).FileName.Trim('"')}";
                _logger.LogInformation($"Generated unique file name: {fileName}");

                using (var stream = file.OpenReadStream())
                {
                    _logger.LogInformation("Opening file stream for upload");
                    var fileUrl = await _blobStorageService.UploadFileAsync(stream, fileName);
                    _logger.LogInformation($"File uploaded successfully. URL: {fileUrl}");
                    return Ok(new { fileUrl });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error uploading file. Details: {ex.Message}");
                if (ex.InnerException != null)
                {
                    _logger.LogError($"Inner exception: {ex.InnerException.Message}");
                }
                return StatusCode(500, new { error = "Internal server error", details = ex.Message });
            }
        }

        // DELETE: api/File/{fileName}
        [HttpDelete("{fileName}")]
        public async Task<IActionResult> DeleteFile(string fileName)
        {
            try
            {
                await _blobStorageService.DeleteFileAsync(fileName);
                return Ok(new { message = "File deleted successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting file");
                return StatusCode(500, "Internal server error");
            }
        }
    }
}
