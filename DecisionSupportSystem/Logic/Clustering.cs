using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DecisionSupportSystem
{
    using Accord.Math;
    using Logic;
    using Microsoft.Research.DynamicDataDisplay.Common;
    using Model;
    using System.ComponentModel;
    using System.Data;
    using System.Threading;
    using System.Windows;
    using System.Windows.Threading;
    public partial class MainWindow
    {
        private string _selectedMetric;
        private string _selectedSimilarityColumn1;
        private string _selectedSimilarityColumn2;
        private string _selectedSimilarityMeasure;
        private List<string> _similarityMeasures;
        private Random rnd = new Random();

        public string SelectedMetric
        {
            get
            {
                return _selectedMetric;
            }
            set
            {
                _selectedMetric = value;
                NotifyPropertyChanged("SelectedMetric");
            }
        }

        public string SelectedSimilarityColumn1
        {
            get
            {
                return _selectedSimilarityColumn1;
            }

            set
            {
                _selectedSimilarityColumn1 = value;
                NotifyPropertyChanged("SelectedSimilarityColumn1");
            }
        }

        public string SelectedSimilarityColumn2
        {
            get
            {
                return _selectedSimilarityColumn2;
            }

            set
            {
                _selectedSimilarityColumn2 = value;
                NotifyPropertyChanged("SelectedSimilarityColumn2");
            }
        }

        public string SelectedSimilarityMeasure
        {
            get
            {
                return _selectedSimilarityMeasure;
            }

            set
            {
                _selectedSimilarityMeasure = value;
                NotifyPropertyChanged("SelectedSimilarityMeasure");
            }
        }

        public List<string> SimilarityMeasures
        {
            get
            {
                return _similarityMeasures;
            }

            set
            {
                _similarityMeasures = value;
                NotifyPropertyChanged("SimilarityMeasures");
            }
        }

        private void InitializeSimilarityMethodValues()
        {
            _similarityMeasures = new List<string>()
            {
                "Jaccard",
                "Dice"
            };
            SelectedSimilarityMeasure = SimilarityMeasures[0];
        }

        private void Clustering_Click(object sender, RoutedEventArgs e)
        {
            int clustersCount;
            if (!int.TryParse(ClusteringClassCount.Text, out clustersCount))
            {
                MessageBox.Show("Wrong Clusters Count!");
                return;
            }

            int iterationsCount;
            if (!int.TryParse(IterationsCount.Text, out iterationsCount))
            {
                MessageBox.Show("Wrong Iterations Count!");
                return;
            }

            Clustering(clustersCount, iterationsCount);
        }

        private void Clustering(int clustersCount, int iterationsCount)
        {
            int colCount = fileData.NumericalColumns.Count;
            // we exclude last column if it's numeric class column
            if (fileData.NumericalColumns.Last() == Data.Columns.Count - 1)
            {
                colCount--;
            }

            double[][] pointVectors = new double[clustersCount][];

            int rowCount = Data.Rows.Count;
            double[,] tabData = new double[rowCount, colCount];

            for (int i = 0; i < rowCount; i++)
            {
                for (int j = 0; j < colCount; j++)
                {
                    tabData[i, j] = Data.Rows[i][fileData.NumericalColumns[j]].toDouble();
                }
            }

            SelectStartingPointsKMeansPP(clustersCount, colCount, pointVectors, rowCount, tabData);

            int[] assignedClusters = new int[rowCount];

            ClusteringKMean(clustersCount, colCount, rowCount, tabData, pointVectors, assignedClusters, iterationsCount);

            string columnName = "Clustering-" + SelectedMetric + " Class#" + clustersCount + " It#" + iterationsCount;
            if (!Data.Columns.Contains(columnName))
            {
                DataColumn newColumn = new DataColumn(columnName, typeof(int));
                Data.Columns.Add(newColumn);
                AxisValues.Add(columnName);
                UpdateAxisComboBoxes();
            }
            for (int i = 0; i < Data.Rows.Count; i++)
            {
                Data.Rows[i][Data.Columns.IndexOf(columnName)] = assignedClusters[i];
            }
            UpdateTable();
        }

        private void SelectStartingPointsKMeansPP(int clustersCount, int colCount, double[][] pointVectors, int rowCount, double[,] tabData)
        {
            List<int> selectedPoints = new List<int>();
            selectedPoints.Add(rnd.Next(rowCount));
            double[] additiveSum = new double[rowCount + 1];
            double dist, minDist, pickVar;

            for (int ptCount = 0; ptCount < clustersCount - 1; ptCount++)
            {                
                for (int i = 1; i < rowCount + 1; i++)
                {
                    minDist = double.MaxValue;
                    for (int j = 0; j < selectedPoints.Count; j++)
                    {
                        dist = Dist(SelectedMetric, i - 1, selectedPoints[j], tabData);
                        if (dist < minDist) minDist = dist;
                    }
                    additiveSum[i] = additiveSum[i - 1] + Math.Pow(minDist, 2);
                }

                pickVar = rnd.NextDouble() * additiveSum[rowCount];
                for (int i = 1; i < rowCount + 1; i++)
                {
                    if (pickVar <= additiveSum[i])
                    {
                        selectedPoints.Add(i - 1);
                        break;
                    }
                }
                additiveSum.Clear();
            }

            for (int i = 0; i < clustersCount; i++)
            {
                pointVectors[i] = new double[colCount];
                for (int j = 0; j < colCount; j++)
                {
                    pointVectors[i][j] = tabData[selectedPoints[i], j];
                }
            }
        }

        private void ClusteringKMean(int clustersCount, int colCount, int rowCount, double[,] tabData, double[][] pointVectors, int[] assignedClusters, int iterationsCount)
        {
            double[][] sums = new double[clustersCount][];
            for (int i = 0; i < clustersCount; i++)
            {
                sums[i] = new double[colCount];
            }
            int[] objCount = new int[clustersCount];

            for (int it = 0; it < iterationsCount; it++)
            {
                for (int i = 0; i < rowCount; i++)
                {
                    int cluster = -1;
                    double dist = double.MaxValue;
                    for (int j = 0; j < clustersCount; j++)
                    {
                        double value = Dist(SelectedMetric, pointVectors[j], i, tabData);
                        if (value < dist)
                        {
                            dist = value;
                            cluster = j;
                            assignedClusters[i] = cluster + 1;
                        }
                    }

                    objCount[cluster]++;
                    for (int j = 0; j < colCount; j++)
                    {
                        sums[cluster][j] += tabData[i, j];
                    }
                }

                for (int i = 0; i < clustersCount; i++)
                {
                    pointVectors[i] = sums[i].Divide(objCount[i]);
                }

                objCount.Clear();
                for (int i = 0; i < clustersCount; i++)
                {
                    sums[i].Clear();
                }
            }
        }

        // using basic clustering ->  Clustering(int clustersCount, int iterationsCount) <- and finding the best solution
        private void Clustering_Optimal_Click(object sender, RoutedEventArgs e)
        {

        }

        // generate table with class comparison result if class count equals original class count
        private void CompareClasses_Click(object sender, RoutedEventArgs e)
        {

        }

        private void CalculateSimilarity_Click(object sender, RoutedEventArgs e)
        {
            int count = Data.Rows.Count;
            string[] column1 = new string[count];
            string[] column2 = new string[count];

            for (int i = 0; i < count; i++)
            {
                column1[i] = Data.Rows[i][SelectedSimilarityColumn1].ToString();
                column2[i] = Data.Rows[i][SelectedSimilarityColumn2].ToString();
            }

            double result = 0;
            switch (SelectedSimilarityMeasure)
            {
                case "Jaccard":
                    result = Jaccard(column1, column2);
                    break;
                case "Dice":
                    result = Dice(column1, column2);
                    break;
            }
            MessageBox.Show(String.Format("Classification Quality equals: {0:0.0000}", result));
        }

        private double Jaccard(string[] column1, string[] column2)
        {
            int smallerSize = column1.Length < column2.Length ? column1.Length : column2.Length;
            int intersectionCount = 0;
            for (int i = 0; i < smallerSize; i++)
            {
                if (column1[i] == column2[i]) intersectionCount++;
            }
            return (double)intersectionCount / (column1.Length + column2.Length - intersectionCount);
        }

        private double Dice(string[] column1, string[] column2)
        {
            int smallerSize = column1.Length < column2.Length ? column1.Length : column2.Length;
            int intersectionCount = 0;
            for (int i = 0; i < smallerSize; i++)
            {
                if (column1[i] == column2[i]) intersectionCount++;
            }
            return (double)(2 * intersectionCount) / (column1.Length + column2.Length);
        }

        // not useful anymore, but leaving just in case
        private void SelectStartingPointsAcross(int clustersCount, int colCount, double[][] pointVectors, int rowCount, double[,] tabData)
        {
            double[] mins = new double[colCount];
            double[] maxs = new double[colCount];

            for (int i = 0; i < colCount; i++)
            {
                mins[i] = double.MaxValue;
                maxs[i] = double.MinValue;
            }

            for (int i = 0; i < rowCount; i++)
            {
                for (int j = 0; j < colCount; j++)
                {
                    double value = tabData[i, j];
                    if (value < mins[j]) mins[j] = value;
                    if (value > maxs[j]) maxs[j] = value;
                }
            }

            double[] diff = new double[colCount];
            for (int i = 0; i < colCount; i++)
            {
                diff[i] = (maxs[i] - mins[i]) / (clustersCount - 1);
            }


            pointVectors[0] = mins;
            for (int i = 1; i < clustersCount; i++)
            {
                pointVectors[i] = new double[colCount];
                for (int j = 0; j < colCount; j++)
                {
                    pointVectors[i][j] = pointVectors[i - 1][j] + diff[j];
                }
            }
        }
    }
}
