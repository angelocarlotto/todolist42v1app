using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using Xunit;
using api.Models;
using api.Controllers;
using System.Net;

namespace api.Tests.Integration
{
    public class ApiIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;
        private readonly HttpClient _client;

        public ApiIntegrationTests(WebApplicationFactory<Program> factory)
        {
            _factory = factory;
            _client = _factory.CreateClient();
        }



        [Fact]
        public async Task Register_CreateUser_ReturnsToken()
        {
            // Arrange
            var request = new RegisterRequest
            {
                Username = $"testuser_{Guid.NewGuid().ToString("N")[..8]}",
                Password = "12345678"
            };

            var json = JsonSerializer.Serialize(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            // Act
            var response = await _client.PostAsync("/api/auth/register", content);

            // Assert
            response.EnsureSuccessStatusCode();
            var responseContent = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<dynamic>(responseContent);
            Assert.NotNull(result);
        }

        [Fact]
        public async Task Login_ValidCredentials_ReturnsToken()
        {
            // Arrange - First register a user
            var username = $"logintest_{Guid.NewGuid().ToString("N")[..8]}";
            var registerRequest = new RegisterRequest
            {
                Username = username,
                Password = "12345678"
            };

            var registerJson = JsonSerializer.Serialize(registerRequest);
            var registerContent = new StringContent(registerJson, Encoding.UTF8, "application/json");
            await _client.PostAsync("/api/auth/register", registerContent);

            // Now login
            var loginRequest = new LoginRequest
            {
                Username = username,
                Password = "12345678"
            };

            var loginJson = JsonSerializer.Serialize(loginRequest);
            var loginContent = new StringContent(loginJson, Encoding.UTF8, "application/json");

            // Act
            var response = await _client.PostAsync("/api/auth/login", loginContent);

            // Assert
            response.EnsureSuccessStatusCode();
            var responseContent = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<dynamic>(responseContent);
            Assert.NotNull(result);
        }

        [Fact]
        public async Task Tasks_UnauthorizedAccess_Returns401()
        {
            // Act
            var response = await _client.GetAsync("/api/tasks");

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task Tasks_AuthorizedAccess_ReturnsTaskList()
        {
            // Arrange - Register and login to get token
            var username = $"tasktest_{Guid.NewGuid().ToString("N")[..8]}";
            var registerRequest = new RegisterRequest
            {
                Username = username,
                Password = "12345678"
            };

            var registerJson = JsonSerializer.Serialize(registerRequest);
            var registerContent = new StringContent(registerJson, Encoding.UTF8, "application/json");
            var registerResponse = await _client.PostAsync("/api/auth/register", registerContent);

            // Login to get token
            var loginRequest = new LoginRequest
            {
                Username = username,
                Password = "12345678"
            };

            var loginJson = JsonSerializer.Serialize(loginRequest);
            var loginContent = new StringContent(loginJson, Encoding.UTF8, "application/json");
            var loginResponse = await _client.PostAsync("/api/auth/login", loginContent);
            var loginResponseContent = await loginResponse.Content.ReadAsStringAsync();
            
            using var loginDoc = JsonDocument.Parse(loginResponseContent);
            var token = loginDoc.RootElement.GetProperty("token").GetString();

            // Add Authorization header
            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            // Act
            var response = await _client.GetAsync("/api/tasks");

            // Assert
            response.EnsureSuccessStatusCode();
            var responseContent = await response.Content.ReadAsStringAsync();
            var tasks = JsonSerializer.Deserialize<List<TaskItem>>(responseContent, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
            Assert.NotNull(tasks);
        }

        [Fact]
        public async Task Tasks_CreateAndRetrieve_WorksCorrectly()
        {
            // Arrange - Register and login to get token
            var username = $"crudtest_{Guid.NewGuid().ToString("N")[..8]}";
            var registerRequest = new RegisterRequest
            {
                Username = username,
                Password = "12345678"
            };

            var registerJson = JsonSerializer.Serialize(registerRequest);
            var registerContent = new StringContent(registerJson, Encoding.UTF8, "application/json");
            var registerResponse = await _client.PostAsync("/api/auth/register", registerContent);

            // Login to get token
            var loginRequest = new LoginRequest
            {
                Username = username,
                Password = "12345678"
            };

            var loginJson = JsonSerializer.Serialize(loginRequest);
            var loginContent = new StringContent(loginJson, Encoding.UTF8, "application/json");
            var loginResponse = await _client.PostAsync("/api/auth/login", loginContent);
            var loginResponseContent = await loginResponse.Content.ReadAsStringAsync();
            
            using var loginDoc = JsonDocument.Parse(loginResponseContent);
            var token = loginDoc.RootElement.GetProperty("token").GetString();

            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            // Create a task - the controller will set tenant/user info from JWT
            var newTask = new TaskItem
            {
                ShortTitle = "Integration Test Task",
                Description = "This is a test task",
                Status = "ToDo",
                Criticality = "Medium",
                DueDate = DateTime.UtcNow.AddDays(1)
            };

            var taskJson = JsonSerializer.Serialize(newTask);
            var taskContent = new StringContent(taskJson, Encoding.UTF8, "application/json");

            // Act - Create task
            var createResponse = await _client.PostAsync("/api/tasks", taskContent);
            
            // Debug: Check response if it fails
            if (!createResponse.IsSuccessStatusCode)
            {
                var errorContent = await createResponse.Content.ReadAsStringAsync();
                throw new Exception($"Failed to create task: {createResponse.StatusCode} - {errorContent}");
            }
            
            createResponse.EnsureSuccessStatusCode();

            var createResponseContent = await createResponse.Content.ReadAsStringAsync();
            var createdTask = JsonSerializer.Deserialize<TaskItem>(createResponseContent, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            // Act - Retrieve tasks
            var getResponse = await _client.GetAsync("/api/tasks");
            getResponse.EnsureSuccessStatusCode();

            var getResponseContent = await getResponse.Content.ReadAsStringAsync();
            var tasks = JsonSerializer.Deserialize<List<TaskItem>>(getResponseContent, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            // Assert
            Assert.NotNull(createdTask);
            Assert.Equal("Integration Test Task", createdTask.ShortTitle);
            Assert.NotNull(tasks);
            Assert.Contains(tasks, t => t.ShortTitle == "Integration Test Task");
        }
    }
}