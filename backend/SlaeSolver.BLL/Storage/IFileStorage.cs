using SlaeSolver.BLL.Models;
using SlaeSolver.BLL.Solver;
namespace SlaeSolver.BLL.Storage;

public interface IFileStorage
{
    Task<string> SaveParametersAsync(Guid taskId, TaskParameters parameters);

    Task<TaskParameters> ReadParametersAsync(string relativePath);

    Task<string> SaveResultAsync(Guid taskId, SolverResult result);

    Task<SolverResult> ReadResultAsync(string relativePath);
}