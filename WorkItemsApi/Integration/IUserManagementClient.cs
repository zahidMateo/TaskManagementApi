using System.Collections.Generic;
using System.Threading.Tasks;
using WorkItemsApi.Models;

namespace WorkItemsApi.Integration
{
    public class UserSaturationDto
    {
        public string UserId { get; set; } = string.Empty;
        public bool IsSaturated { get; set; }
        public int ActiveHighlyRelevantCount { get; set; }
    }

    public interface IUserManagementClient
    {
        Task<IEnumerable<UserDto>> GetUsersAsync();
        Task<UserDto?> GetUserByIdAsync(string id);
        Task<UserSaturationDto?> GetUserSaturationAsync(string userId);
    }
}
