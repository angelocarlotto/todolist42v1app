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
            if (task == null) return NotFound(new { error = "Task not found or no longer shared." });

            // Check if share has expired
            if (task.ShareExpiresAt != null && task.ShareExpiresAt < System.DateTime.UtcNow)
            {
                return StatusCode(410, new { error = "This share link has expired." }); // 410 Gone
            }

            // Check if max views exceeded
            if (task.ShareMaxViews != null && task.ShareViewCount >= task.ShareMaxViews.Value)
            {
                return StatusCode(403, new { error = "This share link has reached its maximum view limit." }); // 403 Forbidden
            }

            // Increment view count
            task.ShareViewCount++;
            await _taskService.UpdateAsync(task.Id!, task);

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
                task.PublicShareId,
                shareExpiresAt = task.ShareExpiresAt,
                shareViewCount = task.ShareViewCount,
                shareMaxViews = task.ShareMaxViews,
                shareAllowEdit = task.ShareAllowEdit
            });
        }

        [HttpPut("{publicShareId}")]
        public async Task<IActionResult> UpdatePublicTask(string publicShareId, [FromBody] PublicTaskUpdateDto updates)
        {
            var task = await _taskService.GetByPublicShareIdAsync(publicShareId);
            if (task == null || task.Id == null) return NotFound(new { error = "Task not found or no longer shared." });

            // Check if share has expired
            if (task.ShareExpiresAt != null && task.ShareExpiresAt < System.DateTime.UtcNow)
            {
                return StatusCode(410, new { error = "This share link has expired." });
            }

            // Check if editing is allowed
            if (!task.ShareAllowEdit)
            {
                return StatusCode(403, new { error = "Editing is not allowed for this share link." });
            }

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
