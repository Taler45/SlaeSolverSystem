namespace SlaeSolver.BLL.DTOs;

public record TaskResultDto(
    Guid TaskId,
    double[] Solution,
    int Iterations,
    double Residual
    );