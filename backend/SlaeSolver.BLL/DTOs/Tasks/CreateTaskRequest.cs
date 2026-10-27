namespace SlaeSolver.BLL.DTOs.Tasks;

public record CreateTaskRequest(
    int MatrixSize, 
    double[][]? Matrix, 
    double[]? RightSide 
    );