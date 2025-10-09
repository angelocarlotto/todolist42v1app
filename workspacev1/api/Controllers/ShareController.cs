using api.Models;
using api.Services;
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
        public ShareController(TaskService taskService)
        {
            _taskService = taskService;
        }

        [HttpPost("{id}/share")]
        public async Task<IActionResult> ShareTask(string id)
        {
            var task = await _taskService.GetAsync(id);
            if (task == null) return NotFound();
            task.PublicShareId = Guid.NewGuid().ToString();
            await _taskService.UpdateAsync(id, task);
            return Ok(new { publicShareId = task.PublicShareId });
        }

        [HttpPost("{id}/revoke-share")]
        public async Task<IActionResult> RevokeShare(string id)
        {
            var task = await _taskService.GetAsync(id);
            if (task == null) return NotFound();
            task.PublicShareId = null;
            await _taskService.UpdateAsync(id, task);
            return Ok();
        }
    }
}
