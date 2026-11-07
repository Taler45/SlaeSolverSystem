namespace SlaeSolver.BLL.DTOs.Tasks;

public record TaskResultDto(
    Guid TaskId,
    double[] Solution,
    int Iterations,
    double Residual
    );