using System.Threading.Tasks;
using WorkItemsApi.Models;

namespace WorkItemsApi.Services
{
    public interface IAssignmentService
    {
        Task<string?> DetermineUserForAssignmentAsync(WorkItem item);
    }
}
