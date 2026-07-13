using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using WorkItemsApi.Data;
using WorkItemsApi.Models;
using WorkItemsApi.Enums;

namespace WorkItemsApi.Services
{
    public class WorkItemService : IWorkItemService
    {
        private readonly WorkItemsDbContext _context;
        private readonly IAssignmentService _assignmentService;
        private readonly ILogger<WorkItemService> _logger;

        public WorkItemService(
            WorkItemsDbContext context,
            IAssignmentService assignmentService,
            ILogger<WorkItemService> logger)
        {
            _context = context;
            _assignmentService = assignmentService;
            _logger = logger;
        }

        /// <summary>
        /// Obtiene todas las tareas registradas en el sistema.
        /// </summary>
        public async Task<IEnumerable<WorkItem>> GetAllWorkItemsAsync()
        {
            return await _context.WorkItems.ToListAsync();
        }

        /// <summary>
        /// Obtiene una tarea específica por su ID.
        /// </summary>
        public async Task<WorkItem?> GetWorkItemByIdAsync(string id)
        {
            return await _context.WorkItems.FindAsync(id);
        }

        /// <summary>
        /// Crea una nueva tarea y reordena la cola de su desarrollador asignado.
        /// </summary>
        public async Task<WorkItem> CreateWorkItemAsync(WorkItem item)
        {
            if (string.IsNullOrEmpty(item.Id))
            {
                item.Id = Guid.NewGuid().ToString();
            }
            _context.WorkItems.Add(item);
            await _context.SaveChangesAsync();

            if (!string.IsNullOrEmpty(item.AssignedUserId))
            {
                await UpdatePendingItemsOrderForUserAsync(item.AssignedUserId);
            }

            return item;
        }

        /// <summary>
        /// Actualiza la información de una tarea y reorganiza las colas de los usuarios afectados.
        /// </summary>
        public async Task<bool> UpdateWorkItemAsync(WorkItem item)
        {
            var existing = await _context.WorkItems.FindAsync(item.Id);
            if (existing == null)
            {
                return false;
            }

            var oldUserId = existing.AssignedUserId;

            existing.Title = item.Title;
            existing.Description = item.Description;
            existing.IsRelevant = item.IsRelevant;
            existing.DueDate = item.DueDate;
            existing.Status = item.Status;
            existing.AssignedUserId = item.AssignedUserId;

            _context.WorkItems.Update(existing);
            await _context.SaveChangesAsync();

            if (!string.IsNullOrEmpty(item.AssignedUserId))
            {
                await UpdatePendingItemsOrderForUserAsync(item.AssignedUserId);
            }
            if (!string.IsNullOrEmpty(oldUserId) && oldUserId != item.AssignedUserId)
            {
                await UpdatePendingItemsOrderForUserAsync(oldUserId);
            }

            return true;
        }

        /// <summary>
        /// Elimina una tarea del sistema y reordena la cola de pendientes del desarrollador afectado.
        /// </summary>
        public async Task<bool> DeleteWorkItemAsync(string id)
        {
            var item = await _context.WorkItems.FindAsync(id);
            if (item == null)
            {
                return false;
            }

            var oldUserId = item.AssignedUserId;
            _context.WorkItems.Remove(item);
            await _context.SaveChangesAsync();

            if (!string.IsNullOrEmpty(oldUserId))
            {
                await UpdatePendingItemsOrderForUserAsync(oldUserId);
            }

            return true;
        }

        /// <summary>
        /// Asigna una tarea de forma manual a un usuario y recalcula los ordenamientos de colas de pendientes.
        /// </summary>
        public async Task<bool> AssignWorkItemAsync(string itemId, string assignedUserId)
        {
            var item = await _context.WorkItems.FindAsync(itemId);
            if (item == null)
            {
                return false;
            }

            var oldUserId = item.AssignedUserId;
            item.AssignedUserId = assignedUserId;
            
            // Set status to Assigned if a user is assigned and status was Pending
            if (!string.IsNullOrEmpty(assignedUserId) && item.Status == Enums.WorkItemStatus.Pending)
            {
                item.Status = Enums.WorkItemStatus.Assigned;
            }
            // Set status back to Pending if unassigned
            else if (string.IsNullOrEmpty(assignedUserId))
            {
                item.Status = Enums.WorkItemStatus.Pending;
            }

            _context.WorkItems.Update(item);
            await _context.SaveChangesAsync();

            if (!string.IsNullOrEmpty(assignedUserId))
            {
                await UpdatePendingItemsOrderForUserAsync(assignedUserId);
            }
            if (!string.IsNullOrEmpty(oldUserId) && oldUserId != assignedUserId)
            {
                await UpdatePendingItemsOrderForUserAsync(oldUserId);
            }

            return true;
        }

        /// <summary>
        /// Auto-asigna de manera inteligente una tarea específica a un usuario no saturado aplicando las reglas.
        /// </summary>
        public async Task<WorkItem?> AutoAssignWorkItemAsync(string itemId)
        {
            var item = await _context.WorkItems.FindAsync(itemId);
            if (item == null)
            {
                _logger.LogWarning("WorkItem {ItemId} not found for auto-assignment.", itemId);
                return null;
            }

            var userId = await _assignmentService.DetermineUserForAssignmentAsync(item);
            if (!string.IsNullOrEmpty(userId))
            {
                item.AssignedUserId = userId;
                item.Status = Enums.WorkItemStatus.Assigned;
                _context.WorkItems.Update(item);
                await _context.SaveChangesAsync();
                
                await UpdatePendingItemsOrderForUserAsync(userId);
                _logger.LogInformation("Successfully auto-assigned WorkItem {ItemId} to User {UserId}.", itemId, userId);
            }
            else
            {
                _logger.LogWarning("Could not determine a user to assign WorkItem {ItemId}.", itemId);
            }

            return item;
        }

        /// <summary>
        /// Auto-asigna todas las tareas huérfanas o sin asignar registradas en el sistema.
        /// </summary>
        public async Task<int> AutoAssignAllUnassignedAsync()
        {
            var unassignedItems = await _context.WorkItems
                .Where(w => string.IsNullOrEmpty(w.AssignedUserId))
                .ToListAsync();

            if (!unassignedItems.Any())
            {
                return 0;
            }

            int count = 0;
            foreach (var item in unassignedItems)
            {
                var assignedItem = await AutoAssignWorkItemAsync(item.Id);
                if (!string.IsNullOrEmpty(assignedItem?.AssignedUserId))
                {
                    count++;
                }
            }

            return count;
        }

        /// <summary>
        /// Obtiene todas las tareas asignadas a un usuario en específico, ordenadas de forma prioritaria por SortOrder.
        /// </summary>
        public async Task<IEnumerable<WorkItem>> GetWorkItemsByUserIdAsync(string userId)
        {
            return await _context.WorkItems
                .Where(w => w.AssignedUserId == userId)
                .OrderBy(w => w.SortOrder)
                .ToListAsync();
        }

        /// <summary>
        /// Desarrolla una función que, después de cada asignación, mantenga ordenada la lista de ítems
        /// pendientes de cada usuario, siguiendo un criterio que priorice relevancia y fechas de entrega.
        /// </summary>
        public async Task UpdatePendingItemsOrderForUserAsync(string userId)
        {
            if (string.IsNullOrEmpty(userId)) return;

            // Obtener todos los ítems asignados al usuario que están pendientes (no Completados)
            var pendingItems = await _context.WorkItems
                .Where(w => w.AssignedUserId == userId && w.Status != WorkItemStatus.Completed)
                .ToListAsync();

            // Ordenar primero por Relevancia (primero las altamente relevantes) y luego por Fecha de Vencimiento (más cercana primero)
            var sortedItems = pendingItems
                .OrderByDescending(w => w.IsRelevant)
                .ThenBy(w => w.DueDate)
                .ToList();

            // Asignar el orden secuencial de clasificación
            for (int i = 0; i < sortedItems.Count; i++)
            {
                sortedItems[i].SortOrder = i + 1;
                _context.WorkItems.Update(sortedItems[i]);
            }

            await _context.SaveChangesAsync();
            _logger.LogInformation("Se ha reordenado la lista de tareas pendientes para el usuario {UserId}. Total tareas ordenadas: {Count}.", userId, sortedItems.Count);
        }
    }
}
