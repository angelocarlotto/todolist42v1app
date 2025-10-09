        
        // ...existing code...

        
using api.Models;
using api.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
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
        public TasksController(TaskService taskService)
        {
            _taskService = taskService;
        }


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
            task.TenantId = tenantId;
            task.CreatedBy = username;
            task.CreatedAt = System.DateTime.UtcNow;
            task.UpdatedBy = username;
            task.UpdatedAt = System.DateTime.UtcNow;
            await _taskService.CreateAsync(task);
            return CreatedAtAction(nameof(Get), new { id = task.Id }, task);
        }


        [HttpPut("{id}")]
        public async Task<IActionResult> Update(string id, TaskItem taskIn)
        {
            var tenantId = User.FindFirst("tenantId")?.Value;
            var username = User.Identity?.Name;
            if (string.IsNullOrEmpty(tenantId) || string.IsNullOrEmpty(username)) return Unauthorized();
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
            await _taskService.UpdateAsync(id, taskIn);
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
            return NoContent();
        }
    }
}
