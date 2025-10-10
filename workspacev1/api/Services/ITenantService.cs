using api.Models;
using System.Threading.Tasks;

namespace api.Services
{
    public interface ITenantService
    {
        Task<Tenant> CreateTenantAsync();
    }
}