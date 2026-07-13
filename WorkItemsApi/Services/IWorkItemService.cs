using System.Collections.Generic;
using System.Threading.Tasks;
using WorkItemsApi.Models;

namespace WorkItemsApi.Services
{
    public interface IWorkItemService
    {
        Task<IEnumerable<WorkItem>> GetAllWorkItemsAsync();
        Task<WorkItem?> GetWorkItemByIdAsync(string id);
        Task<WorkItem> CreateWorkItemAsync(WorkItem item);
        Task<bool> UpdateWorkItemAsync(WorkItem item);
        Task<bool> DeleteWorkItemAsync(string id);
        Task<bool> AssignWorkItemAsync(string itemId, string assignedUserId);
        Task<WorkItem?> AutoAssignWorkItemAsync(string itemId);
        Task<int> AutoAssignAllUnassignedAsync();
        Task<IEnumerable<WorkItem>> GetWorkItemsByUserIdAsync(string userId);
        Task UpdatePendingItemsOrderForUserAsync(string userId);
    }
}
