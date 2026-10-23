namespace SlaeSolver.Domain.Exceptions;

public class InvalidCredentialsException: Exception
{
    
    public InvalidCredentialsException() : base() { }
    public InvalidCredentialsException(string message) : base(message) { }
}