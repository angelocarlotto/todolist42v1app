using api.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace api.Services
{
    public interface ITaskService
    {
        Task<List<TaskItem>> GetTasksByTenantAsync(string tenantId);
        Task<TaskItem> GetTaskByIdAsync(string id, string tenantId);
        Task<TaskItem> CreateTaskAsync(TaskItem task);
        Task<TaskItem> UpdateTaskAsync(string id, TaskItem task, string tenantId);
        Task<bool> DeleteTaskAsync(string id, string tenantId);
    }
}