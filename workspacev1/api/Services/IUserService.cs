using api.Models;
using System.Threading.Tasks;

namespace api.Services
{
    public interface IUserService
    {
        Task<User> CreateUserAsync(User user, string tenantId);
        Task<User> GetUserByEmailAsync(string email);
    }
}