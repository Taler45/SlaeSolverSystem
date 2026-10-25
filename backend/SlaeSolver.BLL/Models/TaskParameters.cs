namespace SlaeSolver.BLL.Models;

public class TaskParameters
{
    public int MatrixSize { get; set; }
    public double[][] Matrix { get; set; } = [];
    public double[] RightSide { get; set; } = [];
}