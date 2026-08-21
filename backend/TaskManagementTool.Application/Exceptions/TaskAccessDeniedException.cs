namespace TaskManagementTool.Application.Exceptions;
public class TaskAccessDeniedException : Exception
{
    public TaskAccessDeniedException()
        : base("You do not have permission to access or modify this task.")
    {
    }
}