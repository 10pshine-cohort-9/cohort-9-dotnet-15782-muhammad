namespace TaskManagementTool.Application.Exceptions;
public class TaskNotFoundException : Exception
{
    public TaskNotFoundException(int taskId)
        : base($"Task with id '{taskId}' was not found.")
    {
    }
}