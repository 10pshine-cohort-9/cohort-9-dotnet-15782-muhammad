namespace TaskManagementTool.Application.Exceptions;

public class DuplicateEmailException : Exception
{
    public DuplicateEmailException(string email)
        : base($"A user with email '{email ?? "unknown"}' already exists.")
    {
    }
}