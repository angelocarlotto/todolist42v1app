using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Moq;
using Xunit;
using api.Controllers;
using api.Models;
using api.Services;
using Microsoft.Extensions.Options;

namespace api.Tests.Controllers
{
    public class AuthControllerTests
    {
        private readonly Mock<UserService> _mockUserService;
        private readonly Mock<TenantService> _mockTenantService;
        private readonly Mock<IConfiguration> _mockConfig;
        private readonly AuthController _controller;

        public AuthControllerTests()
        {
            var mockDbSettings = new Mock<IOptions<DatabaseSettings>>();
            mockDbSettings.Setup(x => x.Value).Returns(new DatabaseSettings
            {
                ConnectionString = "mongodb://localhost:27018",
                DatabaseName = "TaskManagementTest",
                TaskCollectionName = "TasksTest"
            });

            _mockUserService = new Mock<UserService>(mockDbSettings.Object);
            _mockTenantService = new Mock<TenantService>(mockDbSettings.Object);
            _mockConfig = new Mock<IConfiguration>();

            // Setup JWT configuration
            _mockConfig.Setup(c => c["Jwt:Key"]).Returns("this_is_a_very_secure_jwt_signing_key_for_task_management_app_2025");

            _controller = new AuthController(_mockUserService.Object, _mockTenantService.Object, _mockConfig.Object);
        }

        [Fact]
        public async Task Register_ValidUser_ReturnsOk()
        {
            // Arrange
            var request = new RegisterRequest
            {
                Username = "testuser",
                Password = "12345678"
            };

            _mockUserService.Setup(s => s.GetByUsernameAsync("testuser")).ReturnsAsync((User)null);
            _mockTenantService.Setup(s => s.CreateAsync(It.IsAny<Tenant>())).Returns(Task.CompletedTask);
            _mockUserService.Setup(s => s.CreateAsync(It.IsAny<User>())).Returns(Task.CompletedTask);

            // Act
            var result = await _controller.Register(request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult.Value);
        }

        [Fact]
        public async Task Register_ExistingUser_ReturnsBadRequest()
        {
            // Arrange
            var request = new RegisterRequest
            {
                Username = "existinguser",
                Password = "12345678"
            };

            var existingUser = new User { Username = "existinguser" };
            _mockUserService.Setup(s => s.GetByUsernameAsync("existinguser")).ReturnsAsync(existingUser);

            // Act
            var result = await _controller.Register(request);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.NotNull(badRequestResult.Value);
        }

        [Fact]
        public async Task Login_ValidCredentials_ReturnsOkWithToken()
        {
            // Arrange
            var request = new LoginRequest
            {
                Username = "testuser",
                Password = "12345678"
            };

            var user = new User
            {
                Id = "userId123",
                Username = "testuser",
                PasswordHash = "Bd+6qlNLcjZCCnDqeWWV8DrIgknJjnIhOW4YqVOW2C4=", // Hash of "12345678"
                TenantId = "tenantId123"
            };

            _mockUserService.Setup(s => s.GetByUsernameAsync("testuser")).ReturnsAsync(user);

            // Act
            var result = await _controller.Login(request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult.Value);
        }

        [Fact]
        public async Task Login_InvalidCredentials_ReturnsUnauthorized()
        {
            // Arrange
            var request = new LoginRequest
            {
                Username = "testuser",
                Password = "wrongpass"
            };

            var user = new User
            {
                Id = "userId123",
                Username = "testuser",
                PasswordHash = "differenthash",
                TenantId = "tenantId123"
            };

            _mockUserService.Setup(s => s.GetByUsernameAsync("testuser")).ReturnsAsync(user);

            // Act
            var result = await _controller.Login(request);

            // Assert
            var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
            Assert.NotNull(unauthorizedResult.Value);
        }

        [Fact]
        public async Task Login_UserNotFound_ReturnsUnauthorized()
        {
            // Arrange
            var request = new LoginRequest
            {
                Username = "nonexistentuser",
                Password = "12345678"
            };

            _mockUserService.Setup(s => s.GetByUsernameAsync("nonexistentuser")).ReturnsAsync((User)null);

            // Act
            var result = await _controller.Login(request);

            // Assert
            var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
            Assert.NotNull(unauthorizedResult.Value);
        }
    }
}