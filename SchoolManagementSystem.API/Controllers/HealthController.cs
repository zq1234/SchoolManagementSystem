using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SchoolManagementSystem.Infrastructure.Data;

namespace SchoolManagementSystem.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class HealthController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<HealthController> _logger;

        public HealthController(ApplicationDbContext context, ILogger<HealthController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult> GetHealth()
        {
            try
            {
                // Check database connectivity
                var canConnect = await _context.Database.CanConnectAsync();
                //var userCount = await _context.Users.CountAsync();
               // var canConnect = userCount >= 0; // If we get here, connection works

                var healthStatus = new
                {
                    status = "Healthy",
                    timestamp = DateTime.UtcNow,
                    database = canConnect ? "Connected" : "Disconnected",
                    version = "1.0.0"
                };

                return Ok(healthStatus);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Health check failed");

                var healthStatus = new
                {
                    status = "Unhealthy",
                    timestamp = DateTime.UtcNow,
                    database = "Disconnected",
                    error = ex.Message,
                    version = "1.0.0"
                };

                return StatusCode(503, healthStatus);
            }
        }
    }
}