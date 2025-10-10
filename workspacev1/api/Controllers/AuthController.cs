using Microsoft.AspNetCore.Mvc;
using api.Models;
using api.Services;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Authorization;

namespace api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly UserService _userService;
        private readonly TenantService _tenantService;

        public AuthController(UserService userService, TenantService tenantService)
        {
            _userService = userService;
            _tenantService = tenantService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            try
            {
                // Check if user already exists
                var existingUser = await _userService.GetByUsernameAsync(request.Username);
                if (existingUser != null)
                {
                    return BadRequest(new { message = "User already exists" });
                }

                // Create default tenant for new user
                var tenant = new Tenant
                {
                    Name = $"{request.Username}'s Organization"
                };
                await _tenantService.CreateAsync(tenant);

                // Hash password
                var passwordHash = HashPassword(request.Password);

                // Create user
                var user = new User
                {
                    Username = request.Username,
                    PasswordHash = passwordHash,
                    TenantId = tenant.Id
                };

                await _userService.CreateAsync(user);

                return Ok(new { message = "User registered successfully", userId = user.Id, tenantId = tenant.Id });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Registration failed", error = ex.Message });
            }
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            try
            {
                // Find user by username
                var user = await _userService.GetByUsernameAsync(request.Username);
                if (user == null)
                {
                    return Unauthorized(new { message = "Invalid username or password" });
                }

                // Verify password
                var passwordHash = HashPassword(request.Password);
                if (user.PasswordHash != passwordHash)
                {
                    return Unauthorized(new { message = "Invalid username or password" });
                }

                // Generate simple token (in production, use proper JWT)
                var token = GenerateSimpleToken(user.Id, user.Username, user.TenantId);

                return Ok(new { 
                    message = "Login successful", 
                    token = token,
                    userId = user.Id, 
                    tenantId = user.TenantId,
                    username = user.Username
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Login failed", error = ex.Message });
            }
        }

        [HttpPost("logout")]
        [Authorize]
        public IActionResult Logout()
        {
            // In a real application, you would invalidate the token
            // For now, we'll just return success
            return Ok(new { message = "Logout successful" });
        }

        [HttpGet("me")]
        [Authorize]
        public async Task<IActionResult> GetCurrentUser()
        {
            try
            {
                var username = User.Identity?.Name;
                if (string.IsNullOrEmpty(username))
                {
                    return Unauthorized();
                }

                var user = await _userService.GetByUsernameAsync(username);
                if (user == null)
                {
                    return NotFound(new { message = "User not found" });
                }

                return Ok(new { 
                    userId = user.Id, 
                    username = user.Username, 
                    tenantId = user.TenantId 
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Failed to get user info", error = ex.Message });
            }
        }

        [HttpPost("change-password")]
        [Authorize]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
        {
            try
            {
                var username = User.Identity?.Name;
                if (string.IsNullOrEmpty(username))
                {
                    return Unauthorized();
                }

                var user = await _userService.GetByUsernameAsync(username);
                if (user == null)
                {
                    return NotFound(new { message = "User not found" });
                }

                // Verify current password
                var currentPasswordHash = HashPassword(request.CurrentPassword);
                if (user.PasswordHash != currentPasswordHash)
                {
                    return BadRequest(new { message = "Current password is incorrect" });
                }

                // Validate new password (8 digits only)
                if (request.NewPassword.Length != 8 || !request.NewPassword.All(char.IsDigit))
                {
                    return BadRequest(new { message = "Password must be exactly 8 digits" });
                }

                // Update password
                user.PasswordHash = HashPassword(request.NewPassword);
                await _userService.CreateAsync(user); // In real code, use update method

                return Ok(new { message = "Password changed successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Password change failed", error = ex.Message });
            }
        }

        private string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(hashedBytes);
            }
        }

        private string GenerateSimpleToken(string userId, string username, string tenantId)
        {
            // Simple base64 encoded token (in production, use proper JWT)
            var tokenData = $"{userId}:{username}:{tenantId}:{DateTime.UtcNow.AddHours(24):O}";
            var tokenBytes = Encoding.UTF8.GetBytes(tokenData);
            return Convert.ToBase64String(tokenBytes);
        }
    }

    public class RegisterRequest
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public class LoginRequest
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public class ChangePasswordRequest
    {
        public string CurrentPassword { get; set; } = string.Empty;
        public string NewPassword { get; set; } = string.Empty;
    }
}
