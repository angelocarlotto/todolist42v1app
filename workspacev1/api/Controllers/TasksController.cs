using api.Models;
using api.Services;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TasksController : ControllerBase
    {
        private readonly TaskService _taskService;
        public TasksController(TaskService taskService)
        {
            _taskService = taskService;
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
            if (string.IsNullOrEmpty(tenantId)) return Unauthorized();
            task.TenantId = tenantId;
            await _taskService.CreateAsync(task);
            return CreatedAtAction(nameof(Get), new { id = task.Id }, task);
        }


        [HttpPut("{id}")]
        public async Task<IActionResult> Update(string id, TaskItem taskIn)
        {
            var tenantId = User.FindFirst("tenantId")?.Value;
            if (string.IsNullOrEmpty(tenantId)) return Unauthorized();
            var task = await _taskService.GetByTenantAndIdAsync(tenantId, id);
            if (task == null) return NotFound();
            taskIn.Id = id;
            taskIn.TenantId = tenantId;
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
