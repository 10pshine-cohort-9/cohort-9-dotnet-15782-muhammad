namespace TaskManagementTool.Application.Exceptions;
public class InvalidTaskReferenceException : Exception
{
    public InvalidTaskReferenceException(string message) : base(message)
    {
    }
}