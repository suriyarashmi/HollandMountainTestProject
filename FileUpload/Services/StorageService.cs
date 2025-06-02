using FileUpload.Models.Entities;
using FrontendApp.Models;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using TestProject.Interfaces;

namespace TestProject.Services
{
    public class StorageService : IStorageService
    {
        private readonly string _storagePath;

        public StorageService()
        {
            _storagePath = Path.Combine(Directory.GetCurrentDirectory(), "UploadedFiles");
            if (!Directory.Exists(_storagePath))
            {
                Directory.CreateDirectory(_storagePath);
            }
        }

        public async Task SaveFileAsync(FileData file, IFormFile formFile)
        {
            var filePath = Path.Combine(_storagePath, formFile.FileName);
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await formFile.CopyToAsync(stream);
            }
        }

        public Task<List<FileMetadata>> GetAllFilesMetadataAsync()
        {
            var files = Directory.GetFiles(_storagePath)
                .Select(filePath =>
                {
                    var fileInfo = new FileInfo(filePath);
                    var sizeKb = Math.Round(fileInfo.Length / 1024.0, 3);
                    var bstTimeZone = TimeZoneInfo.FindSystemTimeZoneById("GMT Standard Time");
                    var dateModifiedBst = TimeZoneInfo.ConvertTimeFromUtc(fileInfo.LastWriteTimeUtc, bstTimeZone);
                    return new FileMetadata
                    {
                        FileName = fileInfo.Name,
                        Size = sizeKb,
                        DateModified = dateModifiedBst
                    };
                })
                .ToList();

            return Task.FromResult(files);
        }
    }
}
