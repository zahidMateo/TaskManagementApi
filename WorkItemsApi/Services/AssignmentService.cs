using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using WorkItemsApi.Data;
using WorkItemsApi.Enums;
using WorkItemsApi.Integration;
using WorkItemsApi.Models;

namespace WorkItemsApi.Services
{
    /// <summary>
    /// Servicio encargado de ejecutar el motor de asignaciones automáticas de ítems de trabajo.
    /// Aplica las reglas de negocio de fecha próxima y de relevancia, excluyendo usuarios saturados.
    /// </summary>
    public class AssignmentService : IAssignmentService
    {
        private readonly IUserManagementClient _userClient;
        private readonly WorkItemsDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AssignmentService> _logger;

        public AssignmentService(
            IUserManagementClient userClient,
            WorkItemsDbContext context,
            IConfiguration configuration,
            ILogger<AssignmentService> logger)
        {
            _userClient = userClient;
            _context = context;
            _configuration = configuration;
            _logger = logger;
        }

        /// <summary>
        /// Determina el usuario óptimo para asignar una tarea basándose en su relevancia, fecha de entrega
        /// y nivel de carga actual, asegurando no asignar tareas a usuarios saturados.
        /// </summary>
        public async Task<string?> DetermineUserForAssignmentAsync(WorkItem item)
        {
            // 1. Get all available users from the User Management microservice
            var users = (await _userClient.GetUsersAsync()).ToList();
            if (!users.Any())
            {
                _logger.LogWarning("No users found in UserManagementApi. Cannot auto-assign work item.");
                return null;
            }

            // 2. Fetch all work items that are active (Pending or Assigned/InProgress)
            var activeItems = await _context.WorkItems
                .Where(w => w.Status != WorkItemStatus.Completed && !string.IsNullOrEmpty(w.AssignedUserId))
                .ToListAsync();

            // 3. Configure threshold for 'close to expiry'
            int closeToExpiryDays = _configuration.GetValue<int>("AssignmentSettings:CloseToExpiryDays", 3);
            
            // Check if the item's due date is close to expiring (within threshold days)
            bool isCloseToExpiry = item.DueDate <= DateTime.UtcNow.AddDays(closeToExpiryDays);

            _logger.LogInformation("Analyzing assignment for WorkItem '{Title}' (Relevant: {IsRelevant}, DueDate: {DueDate:yyyy-MM-dd}). IsCloseToExpiry: {IsCloseToExpiry} (Threshold: {Threshold} days)",
                item.Title, item.IsRelevant, item.DueDate, isCloseToExpiry, closeToExpiryDays);

            // 4. Calcular cargas de trabajo para cada usuario
            var userWorkloads = users.Select(user =>
            {
                var userActiveItems = activeItems.Where(wi => wi.AssignedUserId == user.Id).ToList();
                int totalActiveCount = userActiveItems.Count; // Pendientes + Asignados
                int pendingCount = userActiveItems.Count(wi => wi.Status == WorkItemStatus.Pending);
                int highlyRelevantActiveCount = userActiveItems.Count(wi => wi.IsRelevant);
                
                bool isSaturated = highlyRelevantActiveCount > 3;

                _logger.LogInformation("Usuario '{UserName}' ({UserId}): TotalActivos={TotalActive}, Pendientes={Pending}, RelevantesActivos={RelevantesActivos}, ¿Saturado?: {IsSaturated}",
                    user.Name, user.Id, totalActiveCount, pendingCount, highlyRelevantActiveCount, isSaturated);

                return new
                {
                    User = user,
                    TotalActiveCount = totalActiveCount,
                    PendingCount = pendingCount,
                    HighlyRelevantActiveCount = highlyRelevantActiveCount,
                    IsSaturated = isSaturated
                };
            }).ToList();

            // Filtrar candidatos que NO estén saturados (ningún usuario con > 3 tareas relevantes activas)
            var candidates = userWorkloads.Where(uw => !uw.IsSaturated).ToList();

            if (!candidates.Any())
            {
                _logger.LogWarning("Todos los usuarios disponibles están saturados (> 3 tareas altamente relevantes activas). No se puede realizar la asignación.");
                return null;
            }

            // 5. Aplicar las reglas de asignación sobre los candidatos no saturados
            string selectedUserId;

            if (isCloseToExpiry)
            {
                // REGLA 1: Entrega próxima -> Asignar al usuario con menos tareas en total, independientemente de relevancia.
                _logger.LogInformation("Aplicando Regla 1 (Fecha Próxima): Asignando al candidato con menos tareas activas totales.");
                var sorted = candidates
                    .OrderBy(uw => uw.TotalActiveCount)
                    .ThenBy(uw => uw.PendingCount)
                    .ThenBy(uw => uw.User.Name)
                    .First();

                selectedUserId = sorted.User.Id;
                _logger.LogInformation("Usuario seleccionado: '{UserName}' con {TotalActiveCount} tareas activas.", sorted.User.Name, sorted.TotalActiveCount);
            }
            else if (item.IsRelevant)
            {
                // REGLA 2: Ítem relevante -> Asignar al candidato con menor backlog de ítems pendientes.
                _logger.LogInformation("Aplicando Regla 2 (Relevante): Asignando al candidato con menos tareas pendientes.");
                var sorted = candidates
                    .OrderBy(uw => uw.PendingCount)
                    .ThenBy(uw => uw.TotalActiveCount)
                    .ThenBy(uw => uw.User.Name)
                    .First();

                selectedUserId = sorted.User.Id;
                _logger.LogInformation("Usuario seleccionado: '{UserName}' con {PendingCount} tareas pendientes.", sorted.User.Name, sorted.PendingCount);
            }
            else
            {
                // REGLA 3: Normal -> Asignar al candidato con menos tareas en total.
                _logger.LogInformation("Aplicando Regla 3 (Asignación Normal): Balanceo de carga por total de tareas.");
                var sorted = candidates
                    .OrderBy(uw => uw.TotalActiveCount)
                    .ThenBy(uw => uw.PendingCount)
                    .ThenBy(uw => uw.User.Name)
                    .First();

                selectedUserId = sorted.User.Id;
                _logger.LogInformation("Usuario seleccionado: '{UserName}' con {TotalActiveCount} tareas activas.", sorted.User.Name, sorted.TotalActiveCount);
            }

            return selectedUserId;
        }
    }
}
