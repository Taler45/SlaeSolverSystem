using SlaeSolver.BLL.Models;

namespace SlaeSolver.BLL.Solver;

public static class SystemGenerator
{
    private const double DominanceRatio = 0.998;
    public static TaskParameters Generate(int n)
    {
        double[] RightSide = new double[n];
        double[][] Matrix = new double[n][];
        Random rnd = Random.Shared;
        
        for (int i = 0; i < n; i++)
        {
            Matrix[i] = new double[n];
            double offSum = 0;
            for (int j = 0; j < n; j++)
            {
                if (j != i)
                {
                    Matrix[i][j] = rnd.NextDouble();
                    offSum += Matrix[i][j];
                }
            }
            Matrix[i][i] = offSum / DominanceRatio + 1;
            RightSide[i] = offSum + Matrix[i][i];
        }

        return new TaskParameters(n, Matrix, RightSide);
    }
}