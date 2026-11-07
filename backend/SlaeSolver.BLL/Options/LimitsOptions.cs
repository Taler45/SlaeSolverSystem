namespace SlaeSolver.BLL.Options;

public class LimitsOptions
{
    public int MaxMatrixSize { get; set; } = 2000;
    public int MaxActiveTasks { get; set; } = 3;
    public int MaxSolveSeconds { get; set; } = 100;
}