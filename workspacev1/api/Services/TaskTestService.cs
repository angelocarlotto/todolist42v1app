using api.Models;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace api.Services
{
    public class TaskTestService : ITaskService
    {
        private readonly TaskService _taskService;

        public TaskTestService(IOptions<DatabaseSettings> dbSettings)
        {
            _taskService = new TaskService(dbSettings);
        }

        public async Task<List<TaskItem>> GetTasksByTenantAsync(string tenantId)
        {
            return await _taskService.GetByTenantAsync(tenantId);
        }

        public async Task<TaskItem> GetTaskByIdAsync(string id, string tenantId)
        {
            return await _taskService.GetByTenantAndIdAsync(tenantId, id);
        }

        public async Task<TaskItem> CreateTaskAsync(TaskItem task)
        {
            await _taskService.CreateAsync(task);
            return task;
        }

        public async Task<TaskItem> UpdateTaskAsync(string id, TaskItem task, string tenantId)
        {
            var existingTask = await _taskService.GetByTenantAndIdAsync(tenantId, id);
            if (existingTask == null) return null;
            
            await _taskService.UpdateAsync(id, task);
            return task;
        }

        public async Task<bool> DeleteTaskAsync(string id, string tenantId)
        {
            var existingTask = await _taskService.GetByTenantAndIdAsync(tenantId, id);
            if (existingTask == null) return false;
            
            await _taskService.RemoveAsync(id);
            return true;
        }
    }
}