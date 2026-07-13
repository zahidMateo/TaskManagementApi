using System.Collections.Generic;
using System.Threading.Tasks;
using WorkItemsApi.Models;

namespace WorkItemsApi.Integration
{
    public interface IUserManagementClient
    {
        Task<IEnumerable<UserDto>> GetUsersAsync();
        Task<UserDto?> GetUserByIdAsync(string id);
    }
}
