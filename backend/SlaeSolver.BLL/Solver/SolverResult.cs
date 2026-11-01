namespace SlaeSolver.BLL.Solver;

public record SolverResult(
    double[] Solution, 
    int Iterations, 
    double Residual, 
    bool Converged
    );