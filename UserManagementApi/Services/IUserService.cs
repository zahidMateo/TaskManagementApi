using System.Collections.Generic;
using System.Threading.Tasks;
using UserManagementApi.Models;

namespace UserManagementApi.Services
{
    public interface IUserService
    {
        Task<IEnumerable<User>> GetAllUsersAsync();
        Task<User?> GetUserByIdAsync(string id);
        Task<User> CreateUserAsync(User user);
        Task<bool> UpdateUserAsync(User user);
        Task<bool> DeleteUserAsync(string id);

        /// <summary>
        /// Determina si un usuario está saturado (tiene más de 3 tareas altamente relevantes activas).
        /// </summary>
        Task<bool> IsUserSaturatedAsync(string id);

        /// <summary>
        /// Obtiene la cantidad de tareas altamente relevantes activas asignadas a un usuario.
        /// </summary>
        Task<int> GetActiveHighlyRelevantTaskCountAsync(string id);
    }
}
