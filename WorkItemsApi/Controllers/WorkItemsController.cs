using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using WorkItemsApi.Models;
using WorkItemsApi.Services;

namespace WorkItemsApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class WorkItemsController : ControllerBase
    {
        private readonly IWorkItemService _workItemService;

        public WorkItemsController(IWorkItemService workItemService)
        {
            _workItemService = workItemService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<WorkItem>>> GetWorkItems()
        {
            var items = await _workItemService.GetAllWorkItemsAsync();
            return Ok(items);
        }

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

        [HttpPost]
        public async Task<ActionResult<WorkItem>> CreateWorkItem(WorkItem item)
        {
            var created = await _workItemService.CreateWorkItemAsync(item);
            return CreatedAtAction(nameof(GetWorkItem), new { id = created.Id }, created);
        }

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

        public class AssignPayload
        {
            public string UserId { get; set; }
        }

        [HttpPost("{id}/assign-to-user")]
        public async Task<IActionResult> AssignWorkItem(string id, [FromBody] AssignPayload payload)
        {
            // If payload is null or userId is null/empty, we unassign
            string targetUserId = payload?.UserId;
            var success = await _workItemService.AssignWorkItemAsync(id, targetUserId);
            if (!success)
            {
                return NotFound($"No se encontró la tarea con ID {id}.");
            }

            return NoContent();
        }

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

        [HttpPost("auto-assign-all")]
        public async Task<ActionResult<object>> AutoAssignAll()
        {
            var count = await _workItemService.AutoAssignAllUnassignedAsync();
            return Ok(new { AssignedCount = count });
        }

        [HttpGet("user/{userId}")]
        public async Task<ActionResult<IEnumerable<WorkItem>>> GetWorkItemsByUser(string userId)
        {
            var items = await _workItemService.GetWorkItemsByUserIdAsync(userId);
            return Ok(items);
        }
    }
}
