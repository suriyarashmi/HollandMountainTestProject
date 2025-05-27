using FileUpload.Services;
using FrontendApp.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging; // Add this for logging
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using TestProject.Models; // Replace with your namespace for FileData and FileDto

namespace FileUploadApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FileController : ControllerBase
    {
        private readonly string _storagePath = Path.Combine(Directory.GetCurrentDirectory(), "UploadedFiles");
        private static readonly ConcurrentDictionary<string, List<FileData>> _filesByRecord = new();
        private readonly ILogger<FileController> _logger; // Logger for logging

        public FileController(ILogger<FileController> logger)
        {
            _logger = logger; // Initialize logger
            if (!Directory.Exists(_storagePath))
            {
                Directory.CreateDirectory(_storagePath);
            }

            // Load existing files from disk into in-memory dictionary (Optional, for persistence after restart)
            LoadExistingFiles();
        }

        // Load existing files from disk (Optional enhancement for POC)
        private void LoadExistingFiles()
        {
            var files = Directory.GetFiles(_storagePath);
            foreach (var filePath in files)
            {
                var fileName = Path.GetFileName(filePath);
                var recordId = "N/A"; // Since RecordId is not saved on disk, you may adjust logic here

                var fileData = new FileData
                {
                    FileName = fileName,
                    RecordId = recordId
                };

                _filesByRecord.AddOrUpdate(recordId,
                    new List<FileData> { fileData },
                    (key, existingList) =>
                    {
                        existingList.Add(fileData);
                        return existingList;
                    });
            }
        }

        // GET: api/file
        [HttpGet]
        public ActionResult<List<FileData>> Get()
        {

            if (!Directory.Exists("UploadedFiles"))
                return Ok(new List<object>()); // return empty list if folder doesn't exist
            var files = Directory.GetFiles(_storagePath)
                .Select(filePath =>
                {
                    var fileInfo = new FileInfo(filePath);
                    var fileSizeInBytes = fileInfo.Length; // Get file size in bytes
                    var fileSizeInKB = fileSizeInBytes / 1024.0; // Convert to KB
                    var RoundfileSizeInKB = Math.Round(fileSizeInKB, 3, MidpointRounding.AwayFromZero);
                    var dateModifiedUtc = fileInfo.LastWriteTimeUtc;
                    // Convert UTC to British Summer Time
                    var bstTimeZone = TimeZoneInfo.FindSystemTimeZoneById("GMT Standard Time");
                    var dateModifiedBst = TimeZoneInfo.ConvertTimeFromUtc(dateModifiedUtc, bstTimeZone);
                    return new FileMetadata
                    {
                        FileName = fileInfo.Name,
                        Size = RoundfileSizeInKB,
                        DateModified = dateModifiedBst
                    };
                })
                .ToList();
            _logger.LogInformation($"Retrieved {files.Count} files."); // Log the count of files
            return Ok(files);
        }

        // POST: api/file/upload
        [HttpPost("upload")]
        [Consumes("multipart/form-data")]
        [RequestSizeLimit(52428800)] // 50 MB limit
        public async Task<IActionResult> UploadFile([FromForm] FileDto dto)
        {
            if (dto == null || dto.File == null || dto.File.Length == 0)
                return BadRequest("No file uploaded.");

            if (string.IsNullOrWhiteSpace(dto.RecordId))
                return BadRequest("RecordId is required.");

            var filePath = Path.Combine(_storagePath, dto.File.FileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await dto.File.CopyToAsync(stream);
            }

            var fileData = new FileData
            {
                FileName = dto.File.FileName,
                RecordId = dto.RecordId
            };

            _filesByRecord.AddOrUpdate(dto.RecordId,
                new List<FileData> { fileData },
                (key, existingList) =>
                {
                    existingList.Add(fileData);
                    return existingList;
                });

            _logger.LogInformation($"File uploaded: {dto.File.FileName} with RecordId: {dto.RecordId}"); // Log upload
            return Ok(new { message = "File uploaded successfully", file = dto.File.FileName });
        }
    }
}
