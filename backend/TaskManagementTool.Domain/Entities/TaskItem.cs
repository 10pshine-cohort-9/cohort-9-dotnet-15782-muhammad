namespace TaskManagementTool.Domain.Entities;

public class TaskItem
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime? DueDate { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public int StatusId { get; set; }
    public Status Status { get; set; } = null!;

    public int PriorityId { get; set; }
    public Priority Priority { get; set; } = null!;

    public int CategoryId { get; set; }
    public Category Category { get; set; } = null!;

    public int CreatedByUserId { get; set; }
    public User CreatedByUser { get; set; } = null!;

    public int AssignedToUserId { get; set; }
    public User AssignedToUser { get; set; } = null!;
}