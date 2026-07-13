using System;
using WorkItemsApi.Enums;

namespace WorkItemsApi.Models
{
    public class WorkItem
    {
        public string Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool IsRelevant { get; set; }
        public DateTime DueDate { get; set; }
        public WorkItemStatus Status { get; set; } = WorkItemStatus.Pending;
        public string AssignedUserId { get; set; }
    }
}
