using System.Collections.Generic;
using System.Threading.Tasks;
using UserManagementApi.Models;

namespace UserManagementApi.Services
{
    /// <summary>
    /// Interfaz para el servicio de gestión de usuarios.
    /// Define las operaciones CRUD y validaciones de carga de trabajo de usuarios.
    /// </summary>
    public interface IUserService
    {
        /// <summary>
        /// Obtiene todos los usuarios registrados en el sistema.
        /// </summary>
        Task<IEnumerable<User>> GetAllUsersAsync();

        /// <summary>
        /// Obtiene los detalles de un usuario específico por su ID.
        /// </summary>
        Task<User?> GetUserByIdAsync(string id);

        /// <summary>
        /// Registra un nuevo usuario en la base de datos.
        /// </summary>
        Task<User> CreateUserAsync(User user);

        /// <summary>
        /// Actualiza la información de un usuario existente.
        /// </summary>
        Task<bool> UpdateUserAsync(User user);

        /// <summary>
        /// Elimina un usuario del sistema por su identificador único.
        /// </summary>
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
