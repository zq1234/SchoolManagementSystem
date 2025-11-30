using Microsoft.AspNetCore.Http;

namespace SchoolManagementSystem.Core.Interfaces
{
    public interface IFileService
    {
        // Single file operations
        Task<string> SaveFileAsync(IFormFile file, string folderPath);
        Task<string> UpdateFileAsync(IFormFile newFile, string existingFilePath, string folderPath);

        // Multiple file operations
        Task<List<string>> SaveFilesAsync(IEnumerable<IFormFile> files, string folderPath);
        Task<List<string>> UpdateFilesAsync(IEnumerable<IFormFile> newFiles, IEnumerable<string> existingFiles, string folderPath);

        // Common operations
        Task<byte[]> GetFileAsync(string filePath);
        bool DeleteFile(string filePath);
        void DeleteFiles(IEnumerable<string> filePaths);
    }

}