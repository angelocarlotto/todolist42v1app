using api.Models;
using api.Services;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ShareController : ControllerBase
    {
    private readonly TaskService _taskService;
    private readonly IHubContext<CollaborationHub> _hubContext;
        public ShareController(TaskService taskService, IHubContext<CollaborationHub> hubContext)
        {
            _taskService = taskService;
            _hubContext = hubContext;
        }

        [HttpPost("{id}/share")]
        public async Task<IActionResult> ShareTask(string id)
        {
            // Use GetByTenantAndIdAsync or similar (tenantId not available here, so fallback to null)
            var task = await _taskService.GetByTenantAndIdAsync((string?)null, id);
            if (task == null) return NotFound();
            task.PublicShareId = Guid.NewGuid().ToString();
            await _taskService.UpdateAsync(id, task);
            // Broadcast to public group
            if (!string.IsNullOrEmpty(task.PublicShareId))
            {
                await _hubContext.Clients.Group(task.PublicShareId).SendAsync("TaskUpdated", task);
            }
            return Ok(new { publicShareId = task.PublicShareId });
        }

        [HttpPost("{id}/revoke-share")]
        public async Task<IActionResult> RevokeShare(string id)
        {
            var task = await _taskService.GetByTenantAndIdAsync((string?)null, id);
            if (task == null) return NotFound();
            var oldShareId = task.PublicShareId;
            task.PublicShareId = null;
            await _taskService.UpdateAsync(id, task);
            // Notify public viewers that the share was revoked
            if (!string.IsNullOrEmpty(oldShareId))
            {
                await _hubContext.Clients.Group(oldShareId).SendAsync("TaskDeleted", id);
            }
            return Ok();
        }
    }
}
