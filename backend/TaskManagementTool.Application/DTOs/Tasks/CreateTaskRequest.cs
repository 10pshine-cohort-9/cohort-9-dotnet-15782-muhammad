namespace TaskManagementTool.Application.DTOs.Tasks
{
    public class CreateTaskRequest
    {
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime? DueDate { get; set; }
        public int? StatusId { get; set; }         // null service defaults to "To Do"
        public int PriorityId { get; set; }
        public int? CategoryId { get; set; }       // null service defaults to "Other" (Id=4)
        public int? AssignedToUserId { get; set; } // null service defaults to self (creator)
    }
}
