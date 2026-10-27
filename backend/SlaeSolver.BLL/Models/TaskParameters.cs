namespace SlaeSolver.BLL.Models;

public record TaskParameters
(
    int MatrixSize,
    double[][] Matrix ,
    double[] RightSide
);