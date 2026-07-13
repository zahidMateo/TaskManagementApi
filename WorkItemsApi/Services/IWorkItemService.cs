using System.Collections.Generic;
using System.Threading.Tasks;
using WorkItemsApi.Models;

namespace WorkItemsApi.Services
{
    /// <summary>
    /// Interfaz para el servicio de gestión de tareas/ítems de trabajo.
    /// Define las operaciones CRUD, lógica de asignación manual y automática, y ordenación de cola.
    /// </summary>
    public interface IWorkItemService
    {
        /// <summary> Obtiene la lista completa de todas las tareas del sistema. </summary>
        Task<IEnumerable<WorkItem>> GetAllWorkItemsAsync();

        /// <summary> Obtiene los detalles de una tarea específica por su ID. </summary>
        Task<WorkItem?> GetWorkItemByIdAsync(string id);

        /// <summary> Crea una nueva tarea y actualiza su prioridad en cola si está asignada. </summary>
        Task<WorkItem> CreateWorkItemAsync(WorkItem item);

        /// <summary> Actualiza los datos de una tarea y reorganiza la cola del usuario asignado. </summary>
        Task<bool> UpdateWorkItemAsync(WorkItem item);

        /// <summary> Elimina una tarea y reordena la cola del usuario que la tenía asignada. </summary>
        Task<bool> DeleteWorkItemAsync(string id);

        /// <summary> Asigna manualmente una tarea a un desarrollador y actualiza las prioridades en cola. </summary>
        Task<bool> AssignWorkItemAsync(string itemId, string assignedUserId);

        /// <summary> Auto-asigna una tarea a un desarrollador aplicando el motor de reglas. </summary>
        Task<WorkItem?> AutoAssignWorkItemAsync(string itemId);

        /// <summary> Auto-asigna todos los ítems que están actualmente sin asignar en el sistema. </summary>
        Task<int> AutoAssignAllUnassignedAsync();

        /// <summary> Obtiene todos los ítems de trabajo asignados a un usuario, ordenados por prioridad de cola. </summary>
        Task<IEnumerable<WorkItem>> GetWorkItemsByUserIdAsync(string userId);

        /// <summary> Reordena de forma persistente la cola de tareas pendientes de un usuario por relevancia y vencimiento. </summary>
        Task UpdatePendingItemsOrderForUserAsync(string userId);
    }
}
