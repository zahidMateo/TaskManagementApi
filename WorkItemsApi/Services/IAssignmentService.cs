using System.Threading.Tasks;
using WorkItemsApi.Models;

namespace WorkItemsApi.Services
{
    /// <summary>
    /// Interfaz para el servicio de asignación automática de tareas.
    /// Contiene las firmas para ejecutar el motor de reglas de negocio.
    /// </summary>
    public interface IAssignmentService
    {
        /// <summary>
        /// Determina el usuario idóneo para la asignación de una tarea de acuerdo con las reglas de negocio de carga y vencimiento.
        /// </summary>
        Task<string?> DetermineUserForAssignmentAsync(WorkItem item);
    }
}
