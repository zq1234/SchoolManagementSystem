// FileUtility.cs
using Microsoft.AspNetCore.Http;

namespace SchoolManagementSystem.API.Utilities
{
    public static class FileUtility
    {
        #region File Validation Constants
        public static class FileLimits
        {
            public const long MaxPhotoSize = 5 * 1024 * 1024; // 5MB
            public const long MaxDocumentSize = 10 * 1024 * 1024; // 10MB
            public const long MaxAssignmentSize = 20 * 1024 * 1024; // 20MB
        }

        public static class FileExtensions
        {
            public static readonly string[] Images = { ".jpg", ".jpeg", ".png", ".gif", ".bmp" };
            public static readonly string[] Documents = { ".pdf", ".doc", ".docx", ".txt", ".rtf" };
            public static readonly string[] Assignments = { ".pdf", ".doc", ".docx", ".txt", ".zip", ".rar", ".ppt", ".pptx" };
            public static readonly string[] AllSupported = Images.Union(Documents).Union(Assignments).Distinct().ToArray();
        }
        #endregion

        #region File Validation Methods
        public static bool IsValidFileExtension(string fileName, string[] allowedExtensions)
        {
            if (string.IsNullOrEmpty(fileName))
                return false;

            var fileExtension = Path.GetExtension(fileName).ToLowerInvariant();
            return allowedExtensions.Contains(fileExtension);
        }

        public static bool IsValidFileSize(long fileSize, long maxSize)
        {
            return fileSize > 0 && fileSize <= maxSize;
        }

        public static (bool isValid, string errorMessage) ValidateFile(IFormFile file, string[] allowedExtensions, long maxSize)
        {
            if (file == null || file.Length == 0)
                return (false, "No file uploaded");

            if (!IsValidFileExtension(file.FileName, allowedExtensions))
                return (false, $"Invalid file type. Allowed types: {string.Join(", ", allowedExtensions)}");

            if (!IsValidFileSize(file.Length, maxSize))
                return (false, $"File size exceeds {maxSize / (1024 * 1024)}MB limit");

            return (true, string.Empty);
        }
        #endregion

        #region Stream Handling Methods
        public static async Task<MemoryStream> CreateMemoryStreamAsync(IFormFile file)
        {
            var memoryStream = new MemoryStream();
            await file.CopyToAsync(memoryStream);
            memoryStream.Position = 0; // Reset stream position to beginning
            return memoryStream;
        }

        public static async Task<MemoryStream> CreateMemoryStreamAsync(Stream stream)
        {
            var memoryStream = new MemoryStream();
            await stream.CopyToAsync(memoryStream);
            memoryStream.Position = 0; // Reset stream position to beginning
            return memoryStream;
        }

        public static async Task<byte[]> ReadAllBytesAsync(IFormFile file)
        {
            using var memoryStream = new MemoryStream();
            await file.CopyToAsync(memoryStream);
            return memoryStream.ToArray();
        }

        public static void DisposeMemoryStream(MemoryStream stream)
        {
            stream?.Dispose();
        }
        #endregion

        #region File Type Specific Validation
        public static (bool isValid, string errorMessage) ValidatePhotoFile(IFormFile file)
        {
            return ValidateFile(file, FileExtensions.Images, FileLimits.MaxPhotoSize);
        }

        public static (bool isValid, string errorMessage) ValidateDocumentFile(IFormFile file)
        {
            return ValidateFile(file, FileExtensions.Documents, FileLimits.MaxDocumentSize);
        }

        public static (bool isValid, string errorMessage) ValidateAssignmentFile(IFormFile file)
        {
            return ValidateFile(file, FileExtensions.Assignments, FileLimits.MaxAssignmentSize);
        }
        #endregion

        #region Content Type Helper
        public static string GetContentType(string fileName)
        {
            var extension = Path.GetExtension(fileName).ToLowerInvariant();
            return extension switch
            {
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".gif" => "image/gif",
                ".bmp" => "image/bmp",
                ".pdf" => "application/pdf",
                ".doc" => "application/msword",
                ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                ".txt" => "text/plain",
                ".rtf" => "application/rtf",
                ".zip" => "application/zip",
                ".rar" => "application/x-rar-compressed",
                ".ppt" => "application/vnd.ms-powerpoint",
                ".pptx" => "application/vnd.openxmlformats-officedocument.presentationml.presentation",
                _ => "application/octet-stream"
            };
        }
        #endregion
    }
}