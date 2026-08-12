namespace TaskManagementTool.Application.DTOs.Tasks
{
    public class TaskResponse
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime? DueDate { get; set; }

        public int StatusId { get; set; }
        public string StatusName { get; set; } = string.Empty;

        public int PriorityId { get; set; }
        public string PriorityName { get; set; } = string.Empty;

        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;

        public int CreatedByUserId { get; set; }
        public string CreatedByUserName { get; set; } = string.Empty;

        public int AssignedToUserId { get; set; }
        public string AssignedToUserName { get; set; } = string.Empty;

        public bool IsAssignedByAdmin => CreatedByUserId != AssignedToUserId; // derived, per Phase 0 decision #2

        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
