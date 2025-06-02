using System.Collections.Generic;
using System.Threading.Tasks;
using FileUpload.Models.Entities;
using FrontendApp.Models;

namespace TestProject.Interfaces
{
    public interface IStorageService
    {
        Task SaveFileAsync(FileData file, IFormFile formFile);
        Task<List<FileMetadata>> GetAllFilesMetadataAsync();
    }
}
