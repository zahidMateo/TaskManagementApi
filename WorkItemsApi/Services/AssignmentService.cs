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

            // 4. Calculate workloads for each user
            var userWorkloads = users.Select(user =>
            {
                var userActiveItems = activeItems.Where(wi => wi.AssignedUserId == user.Id).ToList();
                int totalActiveCount = userActiveItems.Count; // Pending + Assigned
                int pendingCount = userActiveItems.Count(wi => wi.Status == WorkItemStatus.Pending);

                _logger.LogInformation("User '{UserName}' ({UserId}): TotalActive={TotalActive}, Pending={Pending}",
                    user.Name, user.Id, totalActiveCount, pendingCount);

                return new
                {
                    User = user,
                    TotalActiveCount = totalActiveCount,
                    PendingCount = pendingCount
                };
            }).ToList();

            // 5. Apply the assignment rules
            string selectedUserId;

            if (isCloseToExpiry)
            {
                // RULE 1: Close to expiry -> Assign to the user with the fewest work items, regardless of relevance.
                _logger.LogInformation("Applying Rule 1 (Close to Expiry): Assigning based on minimum total active work items.");
                var sorted = userWorkloads
                    .OrderBy(uw => uw.TotalActiveCount)
                    .ThenBy(uw => uw.PendingCount)
                    .ThenBy(uw => uw.User.Name)
                    .First();

                selectedUserId = sorted.User.Id;
                _logger.LogInformation("Selected User: '{UserName}' with {TotalActiveCount} active items.", sorted.User.Name, sorted.TotalActiveCount);
            }
            else if (item.IsRelevant)
            {
                // RULE 2: Relevant (and not close to expiry) -> Assign to user with the shortest backlog of pending items.
                _logger.LogInformation("Applying Rule 2 (Relevant): Assigning based on minimum pending items.");
                var sorted = userWorkloads
                    .OrderBy(uw => uw.PendingCount)
                    .ThenBy(uw => uw.TotalActiveCount)
                    .ThenBy(uw => uw.User.Name)
                    .First();

                selectedUserId = sorted.User.Id;
                _logger.LogInformation("Selected User: '{UserName}' with {PendingCount} pending items.", sorted.User.Name, sorted.PendingCount);
            }
            else
            {
                // RULE 3: Normal items -> Load balance by assigning to the user with the fewest total active tasks
                _logger.LogInformation("Applying Rule 3 (Normal Assignment): Assigning based on total active tasks.");
                var sorted = userWorkloads
                    .OrderBy(uw => uw.TotalActiveCount)
                    .ThenBy(uw => uw.PendingCount)
                    .ThenBy(uw => uw.User.Name)
                    .First();

                selectedUserId = sorted.User.Id;
                _logger.LogInformation("Selected User: '{UserName}' with {TotalActiveCount} active items.", sorted.User.Name, sorted.TotalActiveCount);
            }

            return selectedUserId;
        }
    }
}
