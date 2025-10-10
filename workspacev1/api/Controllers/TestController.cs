using Microsoft.AspNetCore.Mvc;

namespace api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TestController : ControllerBase
    {
        [HttpGet]
        public IActionResult Get()
        {
            return Ok(new { message = "API is working", timestamp = DateTime.UtcNow });
        }

        [HttpGet("auth")]
        [Microsoft.AspNetCore.Authorization.Authorize]
        public IActionResult GetAuth()
        {
            var tenantId = User.FindFirst("tenantId")?.Value;
            var username = User.Identity?.Name;
            return Ok(new { 
                message = "Auth is working", 
                tenantId = tenantId,
                username = username,
                timestamp = DateTime.UtcNow 
            });
        }
    }
}