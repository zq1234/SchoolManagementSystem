using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace SchoolManagementSystem.Application.DTOs.Shared
{
    public class ApiResponse<T>
    {
        public bool Success { get; set; } = true;
        public string Message { get; set; } = "Request successful";
        public T? Data { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        public ApiResponse() { }

        public ApiResponse(T data, string? message = null)
        {
            Data = data;
            if (!string.IsNullOrEmpty(message))
                Message = message;
        }

        // Add error method
        public static ApiResponse<T> Error(string errorMessage)
        {
            return new ApiResponse<T>
            {
                Success = false,
                Message = errorMessage,
                Data = default
            };
        }
    }

    public class ErrorResponse
    {
        public bool Success { get; set; } = false;
        public string Message { get; set; } = "An error occurred";
        public int StatusCode { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public string? StackTrace { get; set; }
        public Dictionary<string, string[]>? Errors { get; set; }

        public ErrorResponse() { }

        public ErrorResponse(string message, int statusCode)
        {
            Success = false;
            Message = message;
            StatusCode = statusCode;
        }
    }

    public class APIResponseDto<T>
    {
        public bool Success { get; set; } = true;
        public string Message { get; set; } = string.Empty;
        public IEnumerable<T> Data { get; set; } = new List<T>();
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }
        public int TotalPages { get; set; }
        public string? PreviousPage { get; set; }
        public string? NextPage { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        public APIResponseDto(IEnumerable<T> data, int page, int pageSize, int totalCount, string baseUrl, string message = "Data Found")
        {
            Success = true;
            Message = message;
            Data = data;
            Page = page;
            PageSize = pageSize;
            TotalCount = totalCount;
            TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
            PreviousPage = page > 1 ? $"{baseUrl}?page={page - 1}&pageSize={pageSize}" : null;
            NextPage = page < TotalPages ? $"{baseUrl}?page={page + 1}&pageSize={pageSize}" : null;
        }

        // Add error constructor
        public APIResponseDto(string errorMessage)
        {
            Success = false;
            Message = errorMessage;
            Data = new List<T>();
            // Pagination properties will have default values (0 for int, null for strings)
        }
    }
}
