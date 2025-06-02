using FileUpload.Models.DTOs;
using FileUpload.Models.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using TestProject.Interfaces;

namespace FileUploadApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DocumentController : ControllerBase
    {
        private readonly ILogger<DocumentController> _logger;
        private readonly IStorageService _storage;

        public DocumentController(ILogger<DocumentController> logger, IStorageService storage)
        {
            _logger = logger;
            _storage = storage;
        }

        // GET: api/document
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var files = await _storage.GetAllFilesMetadataAsync();
            _logger.LogInformation($"Retrieved {files.Count} files.");
            return Ok(files);
        }

        // POST: api/document/upload
        [HttpPost("upload")]
        [Consumes("multipart/form-data")]
        [RequestSizeLimit(52428800)] // 50 MB limit
        public async Task<IActionResult> UploadFile([FromForm] FileDto dto)
        {
            if (dto == null || dto.File == null || dto.File.Length == 0)
                return BadRequest("No file uploaded.");

            if (string.IsNullOrWhiteSpace(dto.RecordId))
                return BadRequest("RecordId is required.");

            var fileData = new FileData
            {
                FileName = dto.File.FileName,
                RecordId = dto.RecordId
            };

            await _storage.SaveFileAsync(fileData, dto.File);

            _logger.LogInformation($"File uploaded: {dto.File.FileName} with RecordId: {dto.RecordId}");
            return Ok(new { message = "File uploaded successfully", file = dto.File.FileName });
        }
    }
}
