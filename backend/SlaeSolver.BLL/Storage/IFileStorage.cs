using SlaeSolver.BLL.Models;

namespace SlaeSolver.BLL.Storage;

public interface IFileStorage
{
    Task<string> SaveParametersAsync(Guid taskId, TaskParameters parameters);

    Task<TaskParameters> ReadParametersAsync(string relativePath);
}