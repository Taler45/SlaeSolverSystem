using SlaeSolver.BLL.Models;
using SlaeSolver.BLL.Options;


namespace SlaeSolver.BLL.Solver;

public class JacobiSolver
{
    public static SolverResult Solve(TaskParameters parameters, Action<int> onProgress,
        CancellationToken cancellationToken, SolverOptions options)
    {
        var matrix = parameters.Matrix;
        var matrixSize = parameters.MatrixSize;
        var matrixRightSide = parameters.RightSide;

        for (int i = 0; i < matrixSize; i++)
        {
            if (matrix[i][i] == 0)
            {
                throw new InvalidOperationException();
            }
        }
        int iterations = 0 ;
        bool converged = false;
        double delta0 = 0 ;
        int lastPercent = -1;

        var oldX = new double[matrixSize];
        var newX = new double[matrixSize];


        while (iterations < options.MaxIterations)
        {
            cancellationToken.ThrowIfCancellationRequested();

            for (int i = 0; i < matrixSize; i++)
            {
                double sum = 0;
                for (int j = 0; j < matrixSize; j++)
                {
                    if (j != i)
                    {
                        sum += matrix[i][j] * oldX[j];
                    }
                }

                newX[i] = (matrixRightSide[i] - sum) / matrix[i][i];
            }

            double maxDelta = 0;
            for (int i = 0; i < matrixSize; i++)
            {
                if (!double.IsFinite(newX[i]))
                {
                    throw new InvalidOperationException(
                        "Метод розбігається: матриця не має діагональної переваги");
                }

                maxDelta = Math.Max(maxDelta, Math.Abs(newX[i] - oldX[i]));
            }

            (oldX, newX) = (newX, oldX);
            iterations++;

            if (maxDelta < options.Epsilon)
            {
                converged = true;
                break;
            }

            if (iterations == 1)
            {
                delta0 = maxDelta;
            }

            int percent = (int)(100 * Math.Log(delta0 / maxDelta) / Math.Log(delta0 / options.Epsilon));
            percent = Math.Clamp(percent, 0, 99);

            if (percent != lastPercent)
            {
                onProgress(percent);
                lastPercent = percent;
            }
        }

        double residual = 0;
        for (int i = 0; i < matrixSize; i++)
        {
            double sum = 0;
            for (int j = 0; j < matrixSize; j++)
            {
                sum += matrix[i][j] * oldX[j];
            }

            residual = Math.Max(residual, Math.Abs(sum - matrixRightSide[i]));
        }

        return new SolverResult(oldX, iterations, residual, converged);
    }
}