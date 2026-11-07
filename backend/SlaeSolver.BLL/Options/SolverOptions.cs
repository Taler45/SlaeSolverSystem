namespace SlaeSolver.BLL.Options;

public class SolverOptions
{
    public double Epsilon { get; set; } = 1e-10;
    public int MaxIterations { get; set; } = 100000;
}