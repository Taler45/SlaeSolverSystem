using SlaeSolver.BLL.Models;

namespace SlaeSolver.BLL.Solver;

public static class SystemGenerator
{
    public static TaskParameters Generate(int n)
    {
        double[] RightSide = new double[n];
        double[][] Matrix = new double[n][];
        Random rnd = new Random(42);
        for (int i = 0; i < n; i++)
        {
            Matrix[i] = new double[n];
            RightSide[i] = rnd.NextDouble() * 100;
            for (int j = 0; j < n; j++)
            {
                Matrix[i][j] = rnd.NextDouble() * 100;
            }
        }

        return new TaskParameters(n, Matrix, RightSide);
    }
}