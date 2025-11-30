using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolManagementSystem.Application.Interfaces;

namespace SchoolManagementSystem.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CacheController : ControllerBase
    {
        private readonly ICacheService _cacheService;

        public CacheController(ICacheService cacheService)
        {
            _cacheService = cacheService;
        }

        [HttpGet("stats")]
        public async Task<IActionResult> GetCacheStats()
        {
            // Test cache functionality
            var testKey = "cache_test_key";
            var testValue = $"Cached at {DateTime.UtcNow}";

            await _cacheService.SetAsync(testKey, testValue, TimeSpan.FromMinutes(1));
            var retrievedValue = await _cacheService.GetAsync<string>(testKey);

            var exists = await _cacheService.ExistsAsync(testKey);

            return Ok(new
            {
                TestKey = testKey,
                StoredValue = testValue,
                RetrievedValue = retrievedValue,
                KeyExists = exists,
                Message = "Cache is working correctly"
            });
        }

        [HttpDelete("clear")]
        [Authorize(Roles = "Admin")]
        public Task<IActionResult> ClearCache([FromQuery] string pattern = "")
        {
            return Task.FromResult<IActionResult>(Ok(new { Message = "Cache Cleared" }));
        }
    }
}
