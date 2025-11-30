using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolManagementSystem.Application.DTOs
{
    public class SubmitDocumentDto
    {
        public IFormFile File { get; set; } = null!;
    }
    public class SubmitDocumentsDto
    {
        //  for multiple file uploads
        public IList<IFormFile> Files { get; set; } = new List<IFormFile>();
    }
}
