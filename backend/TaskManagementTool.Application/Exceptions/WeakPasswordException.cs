namespace TaskManagementTool.Application.Exceptions;

public class WeakPasswordException : Exception
{
    public WeakPasswordException()
        : base("Password must contain at least one uppercase letter, one lowercase letter, and one special character.")
    {
    }
}