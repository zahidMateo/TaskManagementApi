using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using WorkItemsApi.Data;
using WorkItemsApi.Models;

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

        public async Task<IEnumerable<WorkItem>> GetAllWorkItemsAsync()
        {
            return await _context.WorkItems.ToListAsync();
        }

        public async Task<WorkItem?> GetWorkItemByIdAsync(string id)
        {
            return await _context.WorkItems.FindAsync(id);
        }

        public async Task<WorkItem> CreateWorkItemAsync(WorkItem item)
        {
            if (string.IsNullOrEmpty(item.Id))
            {
                item.Id = Guid.NewGuid().ToString();
            }
            _context.WorkItems.Add(item);
            await _context.SaveChangesAsync();
            return item;
        }

        public async Task<bool> UpdateWorkItemAsync(WorkItem item)
        {
            var existing = await _context.WorkItems.FindAsync(item.Id);
            if (existing == null)
            {
                return false;
            }

            existing.Title = item.Title;
            existing.Description = item.Description;
            existing.IsRelevant = item.IsRelevant;
            existing.DueDate = item.DueDate;
            existing.Status = item.Status;
            existing.AssignedUserId = item.AssignedUserId;

            _context.WorkItems.Update(existing);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteWorkItemAsync(string id)
        {
            var item = await _context.WorkItems.FindAsync(id);
            if (item == null)
            {
                return false;
            }

            _context.WorkItems.Remove(item);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> AssignWorkItemAsync(string itemId, string assignedUserId)
        {
            var item = await _context.WorkItems.FindAsync(itemId);
            if (item == null)
            {
                return false;
            }

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
            return true;
        }

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
                _logger.LogInformation("Successfully auto-assigned WorkItem {ItemId} to User {UserId}.", itemId, userId);
            }
            else
            {
                _logger.LogWarning("Could not determine a user to assign WorkItem {ItemId}.", itemId);
            }

            return item;
        }

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

        public async Task<IEnumerable<WorkItem>> GetWorkItemsByUserIdAsync(string userId)
        {
            return await _context.WorkItems
                .Where(w => w.AssignedUserId == userId)
                .ToListAsync();
        }
    }
}
