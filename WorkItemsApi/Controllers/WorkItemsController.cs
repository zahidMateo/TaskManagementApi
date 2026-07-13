using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using WorkItemsApi.Models;
using WorkItemsApi.Services;

namespace WorkItemsApi.Controllers
{
    /// <summary>
    /// Controlador REST para gestionar operaciones con ítems de trabajo (tareas).
    /// Expone endpoints para operaciones CRUD, asignación manual y asignación automática.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class WorkItemsController : ControllerBase
    {
        private readonly IWorkItemService _workItemService;

        /// <summary>
        /// Inicializa una nueva instancia del controlador de ítems de trabajo.
        /// </summary>
        public WorkItemsController(IWorkItemService workItemService)
        {
            _workItemService = workItemService;
        }

        /// <summary>
        /// Obtiene la lista completa de todos los ítems de trabajo.
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<WorkItem>>> GetWorkItems()
        {
            var items = await _workItemService.GetAllWorkItemsAsync();
            return Ok(items);
        }

        /// <summary>
        /// Obtiene los detalles de un ítem de trabajo específico por su ID.
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<WorkItem>> GetWorkItem(string id)
        {
            var item = await _workItemService.GetWorkItemByIdAsync(id);
            if (item == null)
            {
                return NotFound();
            }
            return Ok(item);
        }

        /// <summary>
        /// Crea un nuevo ítem de trabajo (tarea).
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<WorkItem>> CreateWorkItem(WorkItem item)
        {
            var created = await _workItemService.CreateWorkItemAsync(item);
            return CreatedAtAction(nameof(GetWorkItem), new { id = created.Id }, created);
        }

        /// <summary>
        /// Actualiza la información de un ítem de trabajo existente.
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateWorkItem(string id, WorkItem item)
        {
            if (id != item.Id)
            {
                return BadRequest("El ID de la tarea no coincide.");
            }

            var updated = await _workItemService.UpdateWorkItemAsync(item);
            if (!updated)
            {
                return NotFound();
            }

            return NoContent();
        }

        /// <summary>
        /// Elimina un ítem de trabajo de la base de datos.
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteWorkItem(string id)
        {
            var deleted = await _workItemService.DeleteWorkItemAsync(id);
            if (!deleted)
            {
                return NotFound();
            }

            return NoContent();
        }

        /// <summary>
        /// Payload para recibir el identificador del usuario al realizar asignaciones.
        /// </summary>
        public class AssignPayload
        {
            public string UserId { get; set; }
        }

        /// <summary>
        /// Asigna manualmente un ítem de trabajo a un usuario específico.
        /// </summary>
        [HttpPost("{id}/assign-to-user")]
        public async Task<IActionResult> AssignWorkItem(string id, [FromBody] AssignPayload payload)
        {
            // If payload is null or userId is null/empty, we unassign
            string targetUserId = payload?.UserId;
            var success = await _workItemService.AssignWorkItemAsync(id, targetUserId);
            if (!success)
            {
                // Traducido
                return NotFound($"No se encontró la tarea con ID {id}.");
            }

            return NoContent();
        }

        /// <summary>
        /// Realiza la asignación automática de una tarea utilizando el motor de reglas de negocio.
        /// </summary>
        [HttpPost("{id}/auto-assign")]
        public async Task<ActionResult<WorkItem>> AutoAssignWorkItem(string id)
        {
            var item = await _workItemService.AutoAssignWorkItemAsync(id);
            if (item == null)
            {
                return NotFound($"No se encontró la tarea con ID {id}.");
            }

            if (string.IsNullOrEmpty(item.AssignedUserId))
            {
                return BadRequest("No se pudo asignar la tarea. Verifica que existan usuarios en el servicio Gestión de Usuarios.");
            }

            return Ok(item);
        }

        /// <summary>
        /// Intenta auto-asignar todas las tareas que están actualmente pendientes y sin asignar.
        /// </summary>
        [HttpPost("auto-assign-all")]
        public async Task<ActionResult<object>> AutoAssignAll()
        {
            var count = await _workItemService.AutoAssignAllUnassignedAsync();
            return Ok(new { AssignedCount = count });
        }

        /// <summary>
        /// Obtiene todas las tareas asignadas a un usuario específico, ordenadas por prioridad de cola de trabajo (SortOrder).
        /// </summary>
        [HttpGet("user/{userId}")]
        public async Task<ActionResult<IEnumerable<WorkItem>>> GetWorkItemsByUser(string userId)
        {
            var items = await _workItemService.GetWorkItemsByUserIdAsync(userId);
            return Ok(items);
        }
    }
}
