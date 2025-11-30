using Humanizer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolManagementSystem.Application.DTOs;
using SchoolManagementSystem.Application.Interfaces;
using SchoolManagementSystem.Core.Entities;
using SchoolManagementSystem.Core.Interfaces;

namespace SchoolManagementSystem.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class FilesController : ControllerBase
    {
        private readonly IFileService _fileService;
        private readonly IStudentService _studentService;
        private readonly ILogger<FilesController> _logger;

        public FilesController(
            IFileService fileService,
            IStudentService studentService,
            ILogger<FilesController> logger)
        {
            _fileService = fileService;
            _studentService = studentService;
            _logger = logger;
        }

       
        [HttpPost("documents")]
        [Authorize(Roles = "Admin,Teacher")]
        public async Task<ActionResult> UploadDocument([FromForm] SubmitDocumentDto submitDocumentDto)
        {
            try
            {
                if (submitDocumentDto.File == null || submitDocumentDto.File.Length == 0)
                {
                    return BadRequest(new { message = "No file uploaded" });
                }
                  
                var filePath = $"uploads/documents";
                var photoUrl = await _fileService.SaveFileAsync(submitDocumentDto.File, filePath);
                _logger.LogInformation("Document uploaded for : {PhotoUrl}", photoUrl);

                return Ok(new { filePath, message = "Document uploaded successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading document");
                return StatusCode(500, new { message = "An error occurred while uploading the document" });
            }
        }

        [HttpGet("{*filePath}")] // ← ADD ASTERISK FOR NESTED PATHS
        public async Task<ActionResult> GetFile(string filePath)
        {
            try
            {
                var fileBytes = await _fileService.GetFileAsync(filePath);
                var contentType = GetContentType(filePath);

                return File(fileBytes, contentType, Path.GetFileName(filePath));
            }
            catch (FileNotFoundException ex)
            {
                _logger.LogWarning(ex, "File not found: {FilePath}", filePath);
                return NotFound(new { message = "File not found" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving file: {FilePath}", filePath);
                return StatusCode(500, new { message = "An error occurred while retrieving the file" });
            }
        }

        
        [HttpDelete("{*filePath}")]
        [Authorize(Roles = "Admin,Teacher")]
        public Task<ActionResult> DeleteFile(string filePath)
        {
            try
            {
                var result =  _fileService.DeleteFile(filePath);
                if (result)
                {
                    return Task.FromResult<ActionResult>(Ok(new { message = "File deleted successfully" }));
                }
                else
                {
                    return Task.FromResult<ActionResult>(NotFound(new { message = "File not found" }));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting file: {FilePath}", filePath);
                return Task.FromResult<ActionResult>(StatusCode(500, new { message = "An error occurred while deleting the file" }));
            }
        }
        

        private static string GetContentType(string path)
        {
            var extension = Path.GetExtension(path).ToLowerInvariant();
            return extension switch
            {
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".pdf" => "application/pdf",
                ".doc" => "application/msword",
                ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                ".txt" => "text/plain",
                _ => "application/octet-stream"
            };
        }
    }
}