namespace TaskManagementTool.Application.DTOs.Tasks
{
    public class UpdateTaskRequest
    {
        public string? Title { get; set; }
        public string? Description { get; set; }
        public DateTime? DueDate { get; set; }
        public int? StatusId { get; set; }
        public int? PriorityId { get; set; }
        public int? CategoryId { get; set; }
        public int? AssignedToUserId { get; set; }
    }
}
