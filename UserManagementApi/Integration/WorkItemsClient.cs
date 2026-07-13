using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace UserManagementApi.Integration
{
    /// <summary>
    /// Modelo de transferencia de datos para representar una tarea del microservicio WorkItemsApi.
    /// </summary>
    public class WorkItemDto
    {
        public string Id { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public bool IsRelevant { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? AssignedUserId { get; set; }
    }

    /// <summary>
    /// Interfaz para el cliente de integración con el microservicio de tareas.
    /// </summary>
    public interface IWorkItemsClient
    {
        /// <summary>
        /// Obtiene todos los ítems de trabajo asignados a un usuario específico desde el microservicio WorkItemsApi.
        /// </summary>
        Task<IEnumerable<WorkItemDto>> GetWorkItemsByUserIdAsync(string userId);
    }

    /// <summary>
    /// Implementación del cliente de integración que realiza llamadas HTTP REST al microservicio de tareas.
    /// </summary>
    public class WorkItemsClient : IWorkItemsClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<WorkItemsClient> _logger;

        public WorkItemsClient(HttpClient httpClient, ILogger<WorkItemsClient> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<IEnumerable<WorkItemDto>> GetWorkItemsByUserIdAsync(string userId)
        {
            try
            {
                var response = await _httpClient.GetFromJsonAsync<List<WorkItemDto>>($"api/workitems/user/{userId}");
                return response ?? new List<WorkItemDto>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error de comunicación: no se pudieron obtener los ítems de trabajo para el usuario {UserId}.", userId);
                return new List<WorkItemDto>();
            }
        }
    }
}
