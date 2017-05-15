using System;

namespace DecisionSupportSystem
{
    using Accord.Math;

    public partial class MainWindow
    {
        private double Dist(string metric, int object1ID, int object2ID, double[,] tabData, int excluded = -1)
        {
            switch (metric)
            {
                case "Euclidean":
                    return Euclidean(object1ID, object2ID, tabData);
                case "Manhattan":
                    return Manhattan(object1ID, object2ID, tabData);
                case "Infinity":
                    return Infinity(object1ID, object2ID, tabData);
                case "Mahalanobis":
                    return Mahalanobis(object1ID, object2ID, tabData, excluded);
            }
            return 0;
        }

        private double Dist(string metric, double[] object1Vec, int object2ID, double[,] tabData,  int excluded = -1)
        {
            switch (metric)
            {
                case "Euclidean":
                    return EuclideanVec(object1Vec, object2ID, tabData);
                case "Manhattan":
                    return ManhattanVec(object1Vec, object2ID, tabData);
                case "Infinity":
                    return InfinityVec(object1Vec, object2ID, tabData);
                case "Mahalanobis":
                    return MahalanobisVec(object1Vec, object2ID, tabData, excluded);
            }
            return 0;
        }

        private double Euclidean(int object1ID, int object2ID, double[,] tabData)
        {
            double sum = 0;
            for (int i = 0; i < tabData.GetLength(1); i++)
            {
                sum += Math.Pow(tabData[object2ID, i] - tabData[object1ID, i], 2);
            }
            return Math.Sqrt(sum);
        }

        private double Manhattan(int object1ID, int object2ID, double[,] tabData)
        {
            double sum = 0;
            for (int i = 0; i < tabData.GetLength(1); i++)
            {
                sum += Math.Abs(tabData[object2ID, i] - tabData[object1ID, i]);
            }
            return sum;
        }

        private double Infinity(int object1ID, int object2ID, double[,] tabData)
        {
            double max = double.MinValue;
            for (int i = 0; i < tabData.GetLength(1); i++)
            {
                double value = Math.Abs(tabData[object2ID, i] - tabData[object1ID, i]);
                if (value > max) max = value;
            }
            return max;
        }

        private double Mahalanobis(int object1ID, int object2ID, double[,] tabData, int excluded = -1)
        {
            int colCount = tabData.GetLength(1);
            int rowCount = tabData.GetLength(0);

            double[] means = new double[colCount];
            for (int i = 0; i < rowCount; i++)
            {
                for (int j = 0; j < colCount; j++)
                {
                    if (i == excluded) continue;
                    means[j] += tabData[i, j];
                }
            }

            for (int i = 0; i < colCount; i++)
            {
                means[i] = means[i] / (double)rowCount;
            }

            double[,] covar = new double[colCount, colCount];
            for (int i = 0; i < colCount; i++)
            {
                for (int j = 0; j < colCount; j++)
                {
                    double sum = 0;
                    for (int n = 0; n < rowCount; n++)
                    {
                        if (n == excluded) continue;
                        sum += (tabData[n, i] - means[i]) * (tabData[n, j] - means[j]);
                    }
                    covar[i, j] = sum / (rowCount - 1);
                }
            }

            double[,] invCovar = covar.Inverse();

            double[] diff = new double[colCount];
            double[,] diffVert = new double[colCount, 1];
            for (int i = 0; i < colCount; i++)
            {

                double val = tabData[object2ID, i] - tabData[object1ID, i];
                diff[i] = val;
                diffVert[i, 0] = val;

            }
            double[] res1 = diff.Multiply(invCovar);
            double[] result = res1.Multiply(diffVert);
            return Math.Sqrt(result[0]);
        }

        private double EuclideanVec(double[] object1Vec, int object2ID, double[,] tabData)
        {
            double sum = 0;
            for (int i = 0; i < tabData.GetLength(1); i++)
            {
                sum += Math.Pow(tabData[object2ID, i] - object1Vec[i], 2);
            }
            return Math.Sqrt(sum);
        }

        private double ManhattanVec(double[] object1Vec, int object2ID, double[,] tabData)
        {
            double sum = 0;
            for (int i = 0; i < tabData.GetLength(1); i++)
            {
                sum += Math.Abs(tabData[object2ID, i] - object1Vec[i]);
            }
            return sum;
        }

        private double InfinityVec(double[] object1Vec, int object2ID, double[,] tabData)
        {
            double max = double.MinValue;
            for (int i = 0; i < tabData.GetLength(1); i++)
            {
                double value = Math.Abs(tabData[object2ID, i] - object1Vec[i]);
                if (value > max) max = value;
            }
            return max;
        }

        private double MahalanobisVec(double[] object1Vec, int object2ID, double[,] tabData, int excluded = -1)
        {
            int colCount = tabData.GetLength(1);
            int rowCount = tabData.GetLength(0);

            double[] means = new double[colCount];
            for (int i = 0; i < rowCount; i++)
            {
                for (int j = 0; j < colCount; j++)
                {
                    if (i == excluded) continue;
                    means[j] += tabData[i, j];
                }
            }

            for (int i = 0; i < colCount; i++)
            {
                means[i] = means[i] / (double)rowCount;
            }

            double[,] covar = new double[colCount, colCount];
            for (int i = 0; i < colCount; i++)
            {
                for (int j = 0; j < colCount; j++)
                {
                    double sum = 0;
                    for (int n = 0; n < rowCount; n++)
                    {
                        if (n == excluded) continue;
                        sum += (tabData[n, i] - means[i]) * (tabData[n, j] - means[j]);
                    }
                    covar[i, j] = sum / (rowCount - 1);
                }
            }

            double[,] invCovar = covar.Inverse();

            double[] diff = new double[colCount];
            double[,] diffVert = new double[colCount, 1];
            for (int i = 0; i < colCount; i++)
            {

                double val = tabData[object2ID, i] - object1Vec[i];
                diff[i] = val;
                diffVert[i, 0] = val;

            }
            double[] res1 = diff.Multiply(invCovar);
            double[] result = res1.Multiply(diffVert);
            return Math.Sqrt(result[0]);
        }
    }
}
