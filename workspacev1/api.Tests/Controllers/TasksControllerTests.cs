using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Moq;
using Xunit;
using System.Security.Claims;
using api.Controllers;
using api.Models;
using api.Services;
using Microsoft.Extensions.Options;

namespace api.Tests.Controllers
{
    public class TasksControllerTests
    {
        private readonly Mock<TaskService> _mockTaskService;
        private readonly TasksController _controller;

        public TasksControllerTests()
        {
            var mockDbSettings = new Mock<IOptions<DatabaseSettings>>();
            mockDbSettings.Setup(x => x.Value).Returns(new DatabaseSettings
            {
                ConnectionString = "mongodb://localhost:27018",
                DatabaseName = "TaskManagementTest",
                TaskCollectionName = "TasksTest"
            });

            // Use a real TaskService instance for simplicity in testing
            _mockTaskService = new Mock<TaskService>(mockDbSettings.Object);
            _controller = new TasksController(_mockTaskService.Object);

            // Setup user context
            var claims = new List<Claim>
            {
                new Claim("tenantId", "tenant1"),
                new Claim("userId", "user1"),
                new Claim(ClaimTypes.NameIdentifier, "user1"),
                new Claim(ClaimTypes.Name, "testuser")
            };
            var identity = new ClaimsIdentity(claims, "test");
            var principal = new ClaimsPrincipal(identity);

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = principal
                }
            };
        }

        [Fact]
        public async Task GetTasks_ValidTenant_ReturnsOkWithTasks()
        {
            // Arrange
            var tasks = new List<TaskItem>
            {
                new TaskItem { Id = "1", ShortTitle = "Task 1", Description = "Description 1", TenantId = "tenant1", UserId = "user1" },
                new TaskItem { Id = "2", ShortTitle = "Task 2", Description = "Description 2", TenantId = "tenant1", UserId = "user1" }
            };

            _mockTaskService.Setup(s => s.GetByTenantAsync("tenant1")).ReturnsAsync(tasks);

            // Act
            var result = await _controller.Get();

            // Assert
            var actionResult = Assert.IsType<ActionResult<List<TaskItem>>>(result);
            var returnedTasks = Assert.IsType<List<TaskItem>>(actionResult.Value);
            Assert.Equal(2, returnedTasks.Count);
        }

        [Fact]
        public async Task GetTask_ExistingTask_ReturnsOkWithTask()
        {
            // Arrange
            var task = new TaskItem { Id = "1", ShortTitle = "Task 1", Description = "Description 1", TenantId = "tenant1", UserId = "user1" };
            _mockTaskService.Setup(s => s.GetByTenantAndIdAsync("tenant1", "1")).ReturnsAsync(task);

            // Act
            var result = await _controller.Get("1");

            // Assert
            var actionResult = Assert.IsType<ActionResult<TaskItem>>(result);
            var returnedTask = Assert.IsType<TaskItem>(actionResult.Value);
            Assert.Equal("1", returnedTask.Id);
        }

        [Fact]
        public async Task GetTask_NonExistentTask_ReturnsNotFound()
        {
            // Arrange
            _mockTaskService.Setup(s => s.GetByTenantAndIdAsync("tenant1", "999")).ReturnsAsync((TaskItem)null);

            // Act
            var result = await _controller.Get("999");

            // Assert
            var actionResult = Assert.IsType<ActionResult<TaskItem>>(result);
            Assert.IsType<NotFoundResult>(actionResult.Result);
        }

        [Fact]
        public async Task CreateTask_ValidTask_ReturnsCreatedAtAction()
        {
            // Arrange
            var newTask = new TaskItem 
            { 
                ShortTitle = "New Task", 
                Description = "New Description",
                Status = "ToDo",
                Criticality = "Medium",
                DueDate = DateTime.UtcNow.AddDays(1)
            };

            _mockTaskService.Setup(s => s.CreateAsync(It.IsAny<TaskItem>())).Returns(Task.CompletedTask);

            // Act
            var result = await _controller.Create(newTask);

            // Assert
            var createdResult = Assert.IsType<CreatedAtActionResult>(result);
            var returnedTask = Assert.IsType<TaskItem>(createdResult.Value);
            Assert.Equal("New Task", returnedTask.ShortTitle);
            Assert.Equal("tenant1", returnedTask.TenantId);
        }

        [Fact]
        public async Task UpdateTask_ExistingTask_ReturnsNoContent()
        {
            // Arrange
            var existingTask = new TaskItem 
            { 
                Id = "1", 
                ShortTitle = "Task 1", 
                Description = "Description 1", 
                TenantId = "tenant1",
                Status = "ToDo",
                Criticality = "Medium"
            };
            var updateTask = new TaskItem 
            { 
                Id = "1", 
                ShortTitle = "Updated Task", 
                Description = "Updated Description",
                Status = "InProgress",
                Criticality = "High",
                DueDate = DateTime.UtcNow.AddDays(1)
            };

            _mockTaskService.Setup(s => s.GetByTenantAndIdAsync("tenant1", "1")).ReturnsAsync(existingTask);
            _mockTaskService.Setup(s => s.UpdateAsync("1", It.IsAny<TaskItem>())).Returns(Task.CompletedTask);

            // Act
            var result = await _controller.Update("1", updateTask);

            // Assert
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task UpdateTask_NonExistentTask_ReturnsNotFound()
        {
            // Arrange
            var updateTask = new TaskItem 
            { 
                Id = "999", 
                ShortTitle = "Updated Task", 
                Description = "Updated Description",
                Status = "ToDo",
                Criticality = "Medium",
                DueDate = DateTime.UtcNow.AddDays(1)
            };
            _mockTaskService.Setup(s => s.GetByTenantAndIdAsync("tenant1", "999")).ReturnsAsync((TaskItem)null);

            // Act
            var result = await _controller.Update("999", updateTask);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task DeleteTask_ExistingTask_ReturnsNoContent()
        {
            // Arrange
            var existingTask = new TaskItem { Id = "1", ShortTitle = "Task 1", TenantId = "tenant1" };
            _mockTaskService.Setup(s => s.GetByTenantAndIdAsync("tenant1", "1")).ReturnsAsync(existingTask);
            _mockTaskService.Setup(s => s.RemoveAsync("1")).Returns(Task.CompletedTask);

            // Act
            var result = await _controller.Delete("1");

            // Assert
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task DeleteTask_NonExistentTask_ReturnsNotFound()
        {
            // Arrange
            _mockTaskService.Setup(s => s.GetByTenantAndIdAsync("tenant1", "999")).ReturnsAsync((TaskItem)null);

            // Act
            var result = await _controller.Delete("999");

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }
    }
}