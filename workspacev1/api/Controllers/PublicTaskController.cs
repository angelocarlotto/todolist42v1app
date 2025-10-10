using api.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace api.Controllers
{
    [ApiController]
    [Route("public/task")]
    public class PublicTaskController : ControllerBase
    {
        private readonly TaskService _taskService;
        private readonly IHubContext<CollaborationHub> _hubContext;
        public PublicTaskController(TaskService taskService, IHubContext<CollaborationHub> hubContext)
        {
            _taskService = taskService;
            _hubContext = hubContext;
        }

        [HttpGet("{publicShareId}")]
        public async Task<IActionResult> GetByPublicShareId(string publicShareId)
        {
            var task = await _taskService.GetByPublicShareIdAsync(publicShareId);
            if (task == null) return NotFound();
            // Only return non-sensitive fields
            return Ok(new {
                task.Id,
                task.ShortTitle,
                task.Description,
                task.Files,
                task.DueDate,
                task.Status,
                task.Tags,
                task.Criticality,
                task.PublicShareId
            });
        }

        [HttpPut("{publicShareId}")]
        public async Task<IActionResult> UpdatePublicTask(string publicShareId, [FromBody] PublicTaskUpdateDto updates)
        {
            var task = await _taskService.GetByPublicShareIdAsync(publicShareId);
            if (task == null || task.Id == null) return NotFound();

            // Allow updating only specific fields
            if (!string.IsNullOrWhiteSpace(updates.Description))
                task.Description = updates.Description;
            if (!string.IsNullOrWhiteSpace(updates.Status) && new[] { "ToDo", "InProgress", "Done" }.Contains(updates.Status))
                task.Status = updates.Status;

            task.UpdatedAt = System.DateTime.UtcNow;
            task.UpdatedBy = "Public User";

            await _taskService.UpdateAsync(task.Id, task);

            // Broadcast to public group and tenant group
            if (!string.IsNullOrEmpty(task.PublicShareId))
            {
                await _hubContext.Clients.Group(task.PublicShareId).SendAsync("TaskUpdated", task);
            }
            if (!string.IsNullOrEmpty(task.TenantId))
            {
                await _hubContext.Clients.Group(task.TenantId).SendAsync("TaskUpdated", task);
            }

            return Ok(new {
                task.Id,
                task.ShortTitle,
                task.Description,
                task.Files,
                task.DueDate,
                task.Status,
                task.Tags,
                task.Criticality,
                task.PublicShareId
            });
        }
    }

    public class PublicTaskUpdateDto
    {
        public string? Description { get; set; }
        public string? Status { get; set; }
    }
}
