using TestProject.Interfaces;
using TestProject.Models;

namespace TestProject.Services
{
    public class StorageService
    {
        public class LocalStorageService : IStorageService
        {
            private readonly string _storagePath = Path.Combine(Directory.GetCurrentDirectory(), "Uploads");
            private readonly List<FileData> _fileStore = new();

            public LocalStorageService()
            {
                Directory.CreateDirectory(_storagePath);
            }

            public async Task SaveFileAsync(IFormFile file, string recordId)
            {
                var filePath = Path.Combine(_storagePath, file.FileName);

                using var stream = new FileStream(filePath, FileMode.Create);
                await file.CopyToAsync(stream);

                _fileStore.Add(new FileData
                {
                    FileName = file.FileName,                    
                    RecordId = recordId
                   
                });
            }

            public Task<IEnumerable<FileData>> GetFilesAsync(string recordId)
            {
                var result = _fileStore.Where(f => f.RecordId == recordId);
                return Task.FromResult(result.AsEnumerable());
            }
        }
    }
}