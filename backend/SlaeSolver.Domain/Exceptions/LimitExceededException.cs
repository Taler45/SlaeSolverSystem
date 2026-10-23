namespace SlaeSolver.Domain.Exceptions;

public class LimitExceededException:Exception
{
    public LimitExceededException() : base() { }
    public LimitExceededException(string message) : base(message) { }
}