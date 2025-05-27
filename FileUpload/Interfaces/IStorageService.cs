using TestProject.Models;

namespace TestProject.Interfaces
{
    public interface IStorageService
    {
        Task SaveFileAsync(IFormFile file, string recordId);
        Task<IEnumerable<FileData>> GetFilesAsync(string recordId);
    }
}
