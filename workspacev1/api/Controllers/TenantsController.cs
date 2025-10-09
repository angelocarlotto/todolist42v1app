using api.Models;
using api.Services;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TenantsController : ControllerBase
    {
        private readonly TenantService _tenantService;
        private readonly UserService _userService;
        public TenantsController(TenantService tenantService, UserService userService)
        {
            _tenantService = tenantService;
            _userService = userService;
        }

        [HttpPost]
        public async Task<IActionResult> CreateTenant([FromBody] Tenant tenant)
        {
            var existing = await _tenantService.GetByNameAsync(tenant.Name);
            if (existing != null) return BadRequest("Tenant name already exists");
            await _tenantService.CreateAsync(tenant);
            return Ok(tenant);
        }

        [HttpGet]
        public async Task<IActionResult> GetTenants()
        {
            var tenants = await _tenantService.GetAsync();
            return Ok(tenants);
        }

        [HttpPost("join")]
        public async Task<IActionResult> JoinTenant([FromBody] JoinTenantRequest req)
        {
            var tenant = await _tenantService.GetByIdAsync(req.TenantId);
            if (tenant == null) return NotFound("Tenant not found");
            var user = await _userService.GetByUsernameAsync(req.Username);
            if (user == null) return NotFound("User not found");
            user.TenantId = req.TenantId;
            await _userService.CreateAsync(user); // In real code, use an update method
            return Ok();
        }
    }

    public class JoinTenantRequest
    {
        public string Username { get; set; }
        public string TenantId { get; set; }
    }
}
