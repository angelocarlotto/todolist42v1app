       
        
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
            // If status is Done and was not previously Done, set completed info
            if (task.Status != "Done" && taskIn.Status == "Done")
            {
                taskIn.CompletedBy = username;
                taskIn.CompletedAt = System.DateTime.UtcNow;
            }
            // Ensure AssignedUsers is not null
            if (taskIn.AssignedUsers == null)
                taskIn.AssignedUsers = new List<string>();
            await _taskService.UpdateAsync(id, taskIn);
            // Broadcast to all users in the tenant group
            await _hubContext.Clients.Group(tenantId).SendAsync("TaskUpdated", taskIn);
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
}
