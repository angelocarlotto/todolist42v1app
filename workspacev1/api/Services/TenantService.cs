using api.Models;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace api.Services
{
    public class TenantService
    {
        private readonly IMongoCollection<Tenant> _tenants;
        public TenantService(IOptions<DatabaseSettings> dbSettings)
        {
            var client = new MongoClient(dbSettings.Value.ConnectionString);
            var database = client.GetDatabase(dbSettings.Value.DatabaseName);
            _tenants = database.GetCollection<Tenant>("Tenants");
        }
        public async Task<List<Tenant>> GetAsync() =>
            await _tenants.Find(_ => true).ToListAsync();
        public async Task<Tenant> GetByIdAsync(string id) =>
            await _tenants.Find(t => t.Id == id).FirstOrDefaultAsync();
        public async Task<Tenant> GetByNameAsync(string name) =>
            await _tenants.Find(t => t.Name == name).FirstOrDefaultAsync();
        public async Task CreateAsync(Tenant tenant) =>
            await _tenants.InsertOneAsync(tenant);
    }
}
