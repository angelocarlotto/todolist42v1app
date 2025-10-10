       
        
        // ...existing code...

        
using api.Models;
using api.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class TasksController : ControllerBase
    {
        private readonly TaskService _taskService;
        private readonly IHubContext<CollaborationHub> _hubContext;
        public TasksController(TaskService taskService, IHubContext<CollaborationHub> hubContext)
        {
            _taskService = taskService;
            _hubContext = hubContext;
        }

 // Assign users to a task (shared assignment)
        [HttpPost("{id}/assign")]
        public async Task<IActionResult> AssignUsers(string id, [FromBody] List<string> userIds)
        {
            var tenantId = User.FindFirst("tenantId")?.Value;
            var username = User.Identity?.Name;
            if (string.IsNullOrEmpty(tenantId) || string.IsNullOrEmpty(username)) return Unauthorized();
            if (userIds == null || userIds.Count == 0) return BadRequest("No user IDs provided");
            var task = await _taskService.GetByTenantAndIdAsync(tenantId, id);
            if (task == null) return NotFound();
            // Only add unique user IDs
            foreach (var uid in userIds)
            {
                if (!task.AssignedUsers.Contains(uid))
                    task.AssignedUsers.Add(uid);
            }
            task.UpdatedBy = username;
            task.UpdatedAt = System.DateTime.UtcNow;
            await _taskService.UpdateAsync(id, task);
            return Ok(task.AssignedUsers);
        }

        // Unassign users from a task
        [HttpPost("{id}/unassign")]
        public async Task<IActionResult> UnassignUsers(string id, [FromBody] List<string> userIds)
        {
            var tenantId = User.FindFirst("tenantId")?.Value;
            var username = User.Identity?.Name;
            if (string.IsNullOrEmpty(tenantId) || string.IsNullOrEmpty(username)) return Unauthorized();
            if (userIds == null || userIds.Count == 0) return BadRequest("No user IDs provided");
            var task = await _taskService.GetByTenantAndIdAsync(tenantId, id);
            if (task == null) return NotFound();
            task.AssignedUsers.RemoveAll(uid => userIds.Contains(uid));
            task.UpdatedBy = username;
            task.UpdatedAt = System.DateTime.UtcNow;
            await _taskService.UpdateAsync(id, task);
            return Ok(task.AssignedUsers);
        }


        // Improved reminders: get and push reminders to assigned users via SignalR
        [HttpGet("reminders")]
        public async Task<ActionResult<List<TaskItem>>> GetReminders()
        {
            var tenantId = User.FindFirst("tenantId")?.Value;
            if (string.IsNullOrEmpty(tenantId)) return Unauthorized();
            var allTasks = await _taskService.GetByTenantAsync(tenantId);
            var now = System.DateTime.UtcNow;
            var soon = now.AddDays(1);
            var reminders = allTasks.FindAll(t => t.Status != "Done" && (t.DueDate <= soon));
            return reminders;
        }

        // POST: api/tasks/send-reminders
        [HttpPost("send-reminders")]
        public async Task<IActionResult> SendReminders([FromServices] Microsoft.AspNetCore.SignalR.IHubContext<CollaborationHub> hubContext)
        {
            var tenantId = User.FindFirst("tenantId")?.Value;
            if (string.IsNullOrEmpty(tenantId)) return Unauthorized();
            var allTasks = await _taskService.GetByTenantAsync(tenantId);
            var now = System.DateTime.UtcNow;
            var soon = now.AddDays(1);
            var reminders = allTasks.FindAll(t => t.Status != "Done" && (t.DueDate <= soon));
            // For each reminder, notify all assigned users (by userId)
            foreach (var task in reminders)
            {
                foreach (var userId in task.AssignedUsers)
                {
                    // Send a SignalR message to the user (client must listen on userId group)
                    await ((IClientProxy)hubContext.Clients.Group(userId)).SendCoreAsync("ReceiveReminder", new object[] {
                        new {
                            taskId = task.Id,
                            title = task.ShortTitle,
                            dueDate = task.DueDate,
                            criticality = task.Criticality
                        }
                    });
                }
            }
            return Ok(new { sent = reminders.Count });
        }


[HttpPost("{id}/upload")]
        public async Task<IActionResult> UploadFile(string id, [FromForm] List<IFormFile> files)
        {
            var tenantId = User.FindFirst("tenantId")?.Value;
            var username = User.Identity?.Name;
            if (string.IsNullOrEmpty(tenantId) || string.IsNullOrEmpty(username)) return Unauthorized();
            var task = await _taskService.GetByTenantAndIdAsync(tenantId, id);
            if (task == null) return NotFound();
            if (files == null || files.Count == 0) return BadRequest("No files uploaded");
            var uploadDir = Path.Combine(Directory.GetCurrentDirectory(), "uploads");
            if (!Directory.Exists(uploadDir)) Directory.CreateDirectory(uploadDir);
            foreach (var file in files)
            {
                var fileName = $"{Guid.NewGuid()}_{Path.GetFileName(file.FileName)}";
                var filePath = Path.Combine(uploadDir, fileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }
                task.Files.Add($"/uploads/{fileName}");
            }
            task.UpdatedBy = username;
            task.UpdatedAt = System.DateTime.UtcNow;
            await _taskService.UpdateAsync(id, task);
            
            // Broadcast task update via SignalR
            await _hubContext.Clients.Group(tenantId).SendAsync("TaskUpdated", task);
            
            return Ok(task.Files);
        }

        [HttpDelete("{id}/files")]
        public async Task<IActionResult> DeleteFile(string id, [FromBody] DeleteFileRequest request)
        {
            var tenantId = User.FindFirst("tenantId")?.Value;
            var username = User.Identity?.Name;
            if (string.IsNullOrEmpty(tenantId) || string.IsNullOrEmpty(username)) return Unauthorized();
            
            var task = await _taskService.GetByTenantAndIdAsync(tenantId, id);
            if (task == null) return NotFound();
            
            if (string.IsNullOrEmpty(request.FilePath)) return BadRequest("FilePath is required");
            
            // Remove file from task
            if (!task.Files.Remove(request.FilePath))
            {
                return NotFound("File not found in task");
            }
            
            // Delete physical file
            try
            {
                var fileName = request.FilePath.Replace("/uploads/", "", StringComparison.OrdinalIgnoreCase);
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "uploads", fileName);
                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting physical file: {ex.Message}");
                // Continue anyway - file removed from task
            }
            
            task.UpdatedBy = username;
            task.UpdatedAt = System.DateTime.UtcNow;
            await _taskService.UpdateAsync(id, task);
            
            // Broadcast task update via SignalR
            await _hubContext.Clients.Group(tenantId).SendAsync("TaskUpdated", task);
            
            return Ok(task.Files);
        }

        [HttpGet]
        public async Task<ActionResult<List<TaskItem>>> Get()
        {
            var tenantId = User.FindFirst("tenantId")?.Value;
            if (string.IsNullOrEmpty(tenantId)) return Unauthorized();
            return await _taskService.GetByTenantAsync(tenantId);
        }


        [HttpGet("{id}")]
        public async Task<ActionResult<TaskItem>> Get(string id)
        {
            var tenantId = User.FindFirst("tenantId")?.Value;
            if (string.IsNullOrEmpty(tenantId)) return Unauthorized();
            var task = await _taskService.GetByTenantAndIdAsync(tenantId, id);
            if (task == null) return NotFound();
            return task;
        }


        [HttpPost]
    public async Task<ActionResult> Create(TaskItem task)
        {
            var tenantId = User.FindFirst("tenantId")?.Value;
            var username = User.Identity?.Name;
            if (string.IsNullOrEmpty(tenantId) || string.IsNullOrEmpty(username)) return Unauthorized();
            // Validation
            var allowedStatus = new[] { "ToDo", "InProgress", "Done" };
            var allowedCriticality = new[] { "Low", "Medium", "High" };
            if (string.IsNullOrWhiteSpace(task.ShortTitle)) return BadRequest("ShortTitle is required");
            if (string.IsNullOrWhiteSpace(task.Description)) return BadRequest("Description is required");
            if (task.DueDate == default) return BadRequest("DueDate is required");
            if (string.IsNullOrWhiteSpace(task.Status) || !allowedStatus.Contains(task.Status)) return BadRequest($"Status must be one of: {string.Join(", ", allowedStatus)}");
            if (string.IsNullOrWhiteSpace(task.Criticality) || !allowedCriticality.Contains(task.Criticality)) return BadRequest($"Criticality must be one of: {string.Join(", ", allowedCriticality)}");
            // Basic input sanitization
            task.ShortTitle = System.Net.WebUtility.HtmlEncode(task.ShortTitle.Trim());
            task.Description = System.Net.WebUtility.HtmlEncode(task.Description.Trim());
            task.TenantId = tenantId;
            task.CreatedBy = username;
            task.CreatedAt = System.DateTime.UtcNow;
            task.UpdatedBy = username;
            task.UpdatedAt = System.DateTime.UtcNow;
            // Ensure AssignedUsers is not null
            if (task.AssignedUsers == null)
                task.AssignedUsers = new List<string>();
            
            // Add creation activity log
            var createActivity = new ActivityLog
            {
                UserId = task.UserId ?? username,
                Username = username,
                ActivityType = "Created",
                Description = $"Created the task"
            };
            task.ActivityLog.Add(createActivity);
            
            await _taskService.CreateAsync(task);
            // Broadcast to all users in the tenant group
            await _hubContext.Clients.Group(tenantId).SendAsync("TaskCreated", task);
            return CreatedAtAction(nameof(Get), new { id = task.Id }, task);
        }


        [HttpPut("{id}")]
    public async Task<IActionResult> Update(string id, TaskItem taskIn)
        {
            var tenantId = User.FindFirst("tenantId")?.Value;
            var username = User.Identity?.Name;
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(tenantId) || string.IsNullOrEmpty(username)) return Unauthorized();
            // Validation
            var allowedStatus = new[] { "ToDo", "InProgress", "Done" };
            var allowedCriticality = new[] { "Low", "Medium", "High" };
            if (string.IsNullOrWhiteSpace(taskIn.ShortTitle)) return BadRequest("ShortTitle is required");
            if (string.IsNullOrWhiteSpace(taskIn.Description)) return BadRequest("Description is required");
            if (taskIn.DueDate == default) return BadRequest("DueDate is required");
            if (string.IsNullOrWhiteSpace(taskIn.Status) || !allowedStatus.Contains(taskIn.Status)) return BadRequest($"Status must be one of: {string.Join(", ", allowedStatus)}");
            if (string.IsNullOrWhiteSpace(taskIn.Criticality) || !allowedCriticality.Contains(taskIn.Criticality)) return BadRequest($"Criticality must be one of: {string.Join(", ", allowedCriticality)}");
            // Basic input sanitization
            taskIn.ShortTitle = System.Net.WebUtility.HtmlEncode(taskIn.ShortTitle.Trim());
            taskIn.Description = System.Net.WebUtility.HtmlEncode(taskIn.Description.Trim());
            var task = await _taskService.GetByTenantAndIdAsync(tenantId, id);
            if (task == null) return NotFound();
            taskIn.Id = id;
            taskIn.TenantId = tenantId;
            taskIn.UpdatedBy = username;
            taskIn.UpdatedAt = System.DateTime.UtcNow;
            
            // Track status change
            bool statusChanged = task.Status != taskIn.Status;
            string oldStatus = task.Status;
            
            // If status is Done and was not previously Done, set completed info
            if (task.Status != "Done" && taskIn.Status == "Done")
            {
                taskIn.CompletedBy = username;
                taskIn.CompletedAt = System.DateTime.UtcNow;
            }
            
            // Preserve existing comments and activity log
            taskIn.Comments = task.Comments;
            taskIn.ActivityLog = task.ActivityLog;
            
            // Add update activity log
            var updateActivity = new ActivityLog
            {
                UserId = userId ?? username,
                Username = username,
                ActivityType = statusChanged ? "StatusChanged" : "Updated",
                Description = statusChanged ? $"Changed status from {oldStatus} to {taskIn.Status}" : "Updated the task",
                OldValue = statusChanged ? oldStatus : null,
                NewValue = statusChanged ? taskIn.Status : null
            };
            taskIn.ActivityLog.Add(updateActivity);
            
            // Ensure AssignedUsers is not null
            if (taskIn.AssignedUsers == null)
                taskIn.AssignedUsers = new List<string>();
            await _taskService.UpdateAsync(id, taskIn);
            // Broadcast to all users in the tenant group
            await _hubContext.Clients.Group(tenantId).SendAsync("TaskUpdated", taskIn);
            // If task is public, broadcast to public group as well
            if (!string.IsNullOrEmpty(taskIn.PublicShareId))
            {
                await _hubContext.Clients.Group(taskIn.PublicShareId).SendAsync("TaskUpdated", taskIn);
            }
            return NoContent();
        }


        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            var tenantId = User.FindFirst("tenantId")?.Value;
            if (string.IsNullOrEmpty(tenantId)) return Unauthorized();
            var task = await _taskService.GetByTenantAndIdAsync(tenantId, id);
            if (task == null) return NotFound();
            await _taskService.RemoveAsync(id);
            // Broadcast to all users in the tenant group
            await _hubContext.Clients.Group(tenantId).SendAsync("TaskDeleted", id);
            return NoContent();
        }
    }

    public class DeleteFileRequest
    {
        public string FilePath { get; set; } = string.Empty;
    }
}
