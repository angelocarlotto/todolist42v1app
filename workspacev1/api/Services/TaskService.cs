using api.Models;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace api.Services
{
    public class TaskService
    {
        private readonly IMongoCollection<TaskItem> _tasks;

        public TaskService(IOptions<DatabaseSettings> dbSettings)
        {
            var client = new MongoClient(dbSettings.Value.ConnectionString);
            var database = client.GetDatabase(dbSettings.Value.DatabaseName);
            _tasks = database.GetCollection<TaskItem>(dbSettings.Value.TaskCollectionName);
        }



        public async Task<List<TaskItem>> GetByTenantAsync(string tenantId) =>
            await _tasks.Find(t => t.TenantId == tenantId).ToListAsync();

        // Allow nullable tenantId for public share
        public async Task<TaskItem> GetByTenantAndIdAsync(string tenantId, string id)
        {
            if (tenantId == null)
            {
                return await _tasks.Find(t => t.Id == id).FirstOrDefaultAsync();
            }
            return await _tasks.Find(t => t.TenantId == tenantId && t.Id == id).FirstOrDefaultAsync();
        }

        public async Task CreateAsync(TaskItem task) =>
            await _tasks.InsertOneAsync(task);

        public async Task UpdateAsync(string id, TaskItem taskIn) =>
            await _tasks.ReplaceOneAsync(t => t.Id == id, taskIn);

        public async Task RemoveAsync(string id) =>
            await _tasks.DeleteOneAsync(t => t.Id == id);

        // Get task by public share ID (for unauthenticated access)
        public async Task<TaskItem> GetByPublicShareIdAsync(string publicShareId) =>
            await _tasks.Find(t => t.PublicShareId == publicShareId).FirstOrDefaultAsync();
    }
}
