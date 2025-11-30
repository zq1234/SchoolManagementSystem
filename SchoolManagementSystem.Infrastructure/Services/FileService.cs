using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SchoolManagementSystem.Core.Exceptions;
using SchoolManagementSystem.Core.Interfaces;
using System.Linq;

namespace SchoolManagementSystem.Infrastructure.Services
{
    public class FileService : IFileService
    {
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<FileService> _logger;
        private readonly IConfiguration _config;

        private readonly List<string> _allowedExtensions;
        private readonly long _maxFileSizeBytes;

        public FileService(
            IWebHostEnvironment environment,
            ILogger<FileService> logger,
            IConfiguration config)
        {
            _environment = environment;
            _logger = logger;
            _config = config;

            _allowedExtensions = _config
                .GetSection("FileUploadSettings:AllowedExtensions")
                .Get<List<string>>();

            var maxSizeMB = _config.GetValue<int>("FileUploadSettings:MaxFileSizeMB");
            _maxFileSizeBytes = maxSizeMB * 1024 * 1024;
        }

        // ======================================================
        // VALIDATION HELPERS (NOW THROW CUSTOM EXCEPTIONS)
        // ======================================================
        private void ValidateFile(IFormFile file)
        {
            if (file == null || file.Length == 0)
                throw new BadRequestException("Uploaded file is empty or missing.");

            var extension = Path.GetExtension(file.FileName).ToLower();

            if (!_allowedExtensions.Contains(extension))
                throw new ValidationException(new Dictionary<string, string[]>
                {
                    { "FileExtension", new[] { $"File type not allowed: {extension}" } }
                });

            if (file.Length > _maxFileSizeBytes)
                throw new ValidationException(new Dictionary<string, string[]>
                {
                    { "FileSize", new[] { $"File exceeds max size of {_maxFileSizeBytes / (1024 * 1024)} MB" } }
                });
        }

        private string BuildFullPath(string folderPath)
        {
            return Path.Combine(_environment.WebRootPath, folderPath.Replace("/", "\\"));
        }

        // ======================================================
        // SAVE ONE FILE
        // ======================================================
        public async Task<string> SaveFileAsync(IFormFile file, string folderPath)
        {
            ValidateFile(file);

            try
            {
                var fullFolderPath = BuildFullPath(folderPath);

                if (!Directory.Exists(fullFolderPath))
                    Directory.CreateDirectory(fullFolderPath);

                string uniqueName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
                string fullPath = Path.Combine(fullFolderPath, uniqueName);

                using var stream = new FileStream(fullPath, FileMode.Create);
                await file.CopyToAsync(stream);

                return $"{folderPath}/{uniqueName}".Replace("\\", "/");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while saving file");
                throw new BadRequestException("An unexpected error occurred during file upload.");
            }
        }

        // ======================================================
        // UPDATE ONE FILE
        // ======================================================
        public async Task<string> UpdateFileAsync(IFormFile newFile, string existingFilePath, string folderPath)
        {
            ValidateFile(newFile);

            try
            {
                if (!string.IsNullOrEmpty(existingFilePath))
                    DeleteFile(existingFilePath);

                return await SaveFileAsync(newFile, folderPath);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while updating file");
                throw new BadRequestException("Unable to update file.");
            }
        }

        // ======================================================
        // SAVE MULTIPLE
        // ======================================================
        public async Task<List<string>> SaveFilesAsync(IEnumerable<IFormFile> files, string folderPath)
        {
            if (files == null || !files.Any())
                throw new BadRequestException("No files were uploaded.");

            List<string> saved = new();

            foreach (var file in files)
            {
                ValidateFile(file);
                saved.Add(await SaveFileAsync(file, folderPath));
            }

            return saved;
        }

        // ======================================================
        // UPDATE MULTIPLE
        // ======================================================
        public async Task<List<string>> UpdateFilesAsync(IEnumerable<IFormFile> newFiles, IEnumerable<string> existingFiles, string folderPath)
        {
            if (existingFiles != null)
                DeleteFiles(existingFiles);

            return await SaveFilesAsync(newFiles, folderPath);
        }

        // ======================================================
        // READ FILE
        // ======================================================
        public async Task<byte[]> GetFileAsync(string filePath)
        {
            try
            {
                string fullPath = Path.Combine(_environment.WebRootPath, filePath.TrimStart('/'));

                if (!File.Exists(fullPath))
                    throw new NotFoundException($"File '{filePath}' was not found.");

                return await File.ReadAllBytesAsync(fullPath);
            }
            catch (NotFoundException)
            {
                throw; // already custom
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while retrieving file");
                throw new BadRequestException("Failed to retrieve file.");
            }
        }

        // ======================================================
        // DELETE ONE
        // ======================================================
        public bool DeleteFile(string filePath)
        {
            try
            {
                string fullPath = Path.Combine(_environment.WebRootPath, filePath.TrimStart('/'));

                if (File.Exists(fullPath))
                {
                    File.Delete(fullPath);
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error deleting file");
                throw new BadRequestException("Unable to delete the file.");
            }
        }

        // ======================================================
        // DELETE MULTIPLE
        // ======================================================
        public void DeleteFiles(IEnumerable<string> filePaths)
        {
            if (filePaths == null) return;

            foreach (var path in filePaths)
                DeleteFile(path);
        }
    }
}
