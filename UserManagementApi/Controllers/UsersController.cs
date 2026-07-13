using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using UserManagementApi.Models;
using UserManagementApi.Services;

namespace UserManagementApi.Controllers
{
    /// <summary>
    /// Controlador REST para gestionar operaciones con usuarios.
    /// Expone endpoints para operaciones CRUD y cálculo de saturación.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;

        /// <summary>
        /// Inicializa una nueva instancia del controlador de usuarios.
        /// </summary>
        public UsersController(IUserService userService)
        {
            _userService = userService;
        }

        /// <summary>
        /// Obtiene la lista completa de todos los usuarios registrados.
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<User>>> GetUsers()
        {
            var users = await _userService.GetAllUsersAsync();
            return Ok(users);
        }

        /// <summary>
        /// Obtiene los detalles de un usuario específico por su ID.
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<User>> GetUser(string id)
        {
            var user = await _userService.GetUserByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }
            return Ok(user);
        }

        /// <summary>
        /// Crea y registra un nuevo usuario en la base de datos.
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<User>> CreateUser(User user)
        {
            var createdUser = await _userService.CreateUserAsync(user);
            return CreatedAtAction(nameof(GetUser), new { id = createdUser.Id }, createdUser);
        }

        /// <summary>
        /// Actualiza la información de un usuario existente.
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(string id, User user)
        {
            if (id != user.Id)
            {
                return BadRequest("El ID del usuario no coincide.");
            }

            var updated = await _userService.UpdateUserAsync(user);
            if (!updated)
            {
                return NotFound();
            }

            return NoContent();
        }

        /// <summary>
        /// Elimina un usuario por su ID de la base de datos.
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(string id)
        {
            var deleted = await _userService.DeleteUserAsync(id);
            if (!deleted)
            {
                return NotFound();
            }

            return NoContent();
        }

        /// <summary>
        /// Obtiene el estado de saturación de un usuario específico.
        /// Un usuario está saturado si tiene más de 3 tareas altamente relevantes activas.
        /// </summary>
        [HttpGet("{id}/saturation")]
        public async Task<IActionResult> GetUserSaturation(string id)
        {
            var user = await _userService.GetUserByIdAsync(id);
            if (user == null)
            {
                return NotFound("El usuario no existe.");
            }

            var isSaturated = await _userService.IsUserSaturatedAsync(id);
            var count = await _userService.GetActiveHighlyRelevantTaskCountAsync(id);

            return Ok(new
            {
                UserId = id,
                IsSaturated = isSaturated,
                ActiveHighlyRelevantCount = count
            });
        }
    }
}
