using api.Services;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace api.Controllers
{
    [ApiController]
    [Route("public/task")]
    public class PublicTaskController : ControllerBase
    {
        private readonly TaskService _taskService;
        public PublicTaskController(TaskService taskService)
        {
            _taskService = taskService;
        }

        [HttpGet("{publicShareId}")]
        public async Task<IActionResult> GetByPublicShareId(string publicShareId)
        {
            var task = await _taskService.GetByPublicShareIdAsync(publicShareId);
            if (task == null) return NotFound();
            // Only return non-sensitive fields
            return Ok(new {
                task.ShortTitle,
                task.Description,
                task.Files,
                task.DueDate,
                task.Status,
                task.Tags,
                task.Criticality
            });
        }
    }
}
