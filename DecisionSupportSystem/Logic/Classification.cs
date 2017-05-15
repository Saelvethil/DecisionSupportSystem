using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace DecisionSupportSystem
{
    using Accord.Math;
    using Logic;
    using Microsoft.Research.DynamicDataDisplay.Common;
    using Model;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Threading;
    using System.Windows.Threading;

    public partial class MainWindow
    {
        private List<string> _metrics;
        private List<string> _allColumnsValues;
        private string _selectedClassificationMethod;
        private string _selectedClassificationColumn;
        private List<RingArray<Point>> outputPoints;

        public List<string> Metrics
        {
            get
            {
                return _metrics;
            }
            set
            {
                _metrics = value;
                NotifyPropertyChanged("Metrics");
            }
        }

        public List<string> AllColumnsValues
        {
            get
            {
                return _allColumnsValues;
            }
            set
            {
                _allColumnsValues = value;
                NotifyPropertyChanged("AllColumnsValues");
            }
        }

        public string SelectedClassificationMethod
        {
            get
            {
                return _selectedClassificationMethod;
            }
            set
            {
                _selectedClassificationMethod = value;
                NotifyPropertyChanged("SelectedClassificationMethod");
            }
        }

        public string SelectedClassificationColumn
        {
            get
            {
                return _selectedClassificationColumn;
            }
            set
            {
                _selectedClassificationColumn = value;
                NotifyPropertyChanged("SelectedClassificationColumn");
            }
        }

        private void InitializeClassificationMethodValues()
        {
            _metrics = new List<string>()
            {
                "Euclidean",
                "Manhattan",
                "Infinity",
                "Mahalanobis"
            };
            SelectedClassificationMethod = Metrics[0];
            SelectedMetric = Metrics[0];
        }

        private void InitializeAllColumnsComboBoxes()
        {
            List<string> columns = new List<string>();
            for (int i = 0; i < Data.Columns.Count; i++)
            {
                string colName = Data.Columns[i].ColumnName;
                columns.Add(colName);
            }

            AllColumnsValues = columns;
            if (AllColumnsValues.Count > 0)
            {
                SelectedRemovalColumn = AllColumnsValues.First();
                SelectedClassificationColumn = AllColumnsValues.First();
            }
        }

        private void UpdateAllColumnValuesComboBoxes()
        {
            var tmp = AllColumnsValues;
            AllColumnsValues = null;
            AllColumnsValues = tmp;
            if (AllColumnsValues.Count > 0)
            {
                SelectedRemovalColumn = AllColumnsValues.First();
                SelectedClassificationColumn = AllColumnsValues.First();
            }
        }

        private void Classificate_Click(object sender, RoutedEventArgs e)
        {
            int neighboursCount;
            if (!int.TryParse(ClassificationNeighbourCount.Text, out neighboursCount))
            {
                MessageBox.Show("Wrong Neighbours Count!");
                return;
            }

            // we classify last row
            int excluded = Data.Rows.Count - 1;
            DataRow row = Data.Rows[excluded];

            double[,] tabData = new double[Data.Rows.Count, AxisValues.Count];
            for (int i = 0; i < tabData.GetLength(0); i++)
            {
                for (int j = 0; j < tabData.GetLength(1); j++)
                {
                    tabData[i, j] = Data.Rows[i][AxisValues[j]].toDouble();
                }
            }

            string designated = Classificate(neighboursCount, excluded, row, tabData);

            row[SelectedClassificationColumn] = designated;

        }

        private void CheckClassificationQualityChart_Click(object sender, RoutedEventArgs e)
        {
            BackgroundWorker worker = new BackgroundWorker();
            worker.WorkerReportsProgress = true;
            worker.DoWork += CheckClassificationQualityChart;
            worker.ProgressChanged += CheckClassificationQualityChart_ProgressChanged;

            worker.RunWorkerAsync();
        }

        private void CheckClassificationQualityChart(object sender, DoWorkEventArgs e)
        {
            Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new ThreadStart(() =>
            {
                progressBar.Visibility = Visibility.Visible;
                (sender as BackgroundWorker).ReportProgress(0);
            }));

            int progress = 0;
            int count = Data.Rows.Count;
            GraphModel model = new GraphModel();
            model.AxisNameX = "Neighbour Count";
            model.AxisNameY = SelectedClassificationMethod + " Classification Quality %";

            outputPoints = new List<RingArray<Point>>();
            outputPoints.Add(new RingArray<Point>(Data.Rows.Count));

            double[,] tabData = new double[Data.Rows.Count, AxisValues.Count];
            for (int i = 0; i < tabData.GetLength(0); i++)
            {
                for (int j = 0; j < tabData.GetLength(1); j++)
                {
                    tabData[i, j] = Data.Rows[i][AxisValues[j]].toDouble();
                }
            }

            Parallel.For(1, Data.Rows.Count - 1, o =>
            {
                outputPoints[0].Add(new Point(o, 100d * ClassificationQualitySimple(o, tabData)));
                progress++;
                Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new ThreadStart(() => { (sender as BackgroundWorker).ReportProgress(100 * progress / count); }));
            });

            model.Points = outputPoints;
            model.MinValueX = 0;
            model.MinValueY = 0;
            model.MaxValueX = Data.Rows.Count;
            model.MaxValueY = 100;

            Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new ThreadStart(() =>
            {
                progressBar.Visibility = Visibility.Collapsed;
                var Window2D = new _2DChart(model, false);
                Window2D.Show();
            }));

        }

        void CheckClassificationQualityChart_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            progressBar.Value = e.ProgressPercentage;
        }

        private void CheckClassificationQuality_Click(object sender, RoutedEventArgs e)
        {
            int neighboursCount;
            if (!int.TryParse(ClassificationNeighbourCount.Text, out neighboursCount))
            {
                MessageBox.Show("Wrong Neighbours Count!");
                return;
            }

            double[,] tabData = new double[Data.Rows.Count, AxisValues.Count];
            for (int i = 0; i < tabData.GetLength(0); i++)
            {
                for (int j = 0; j < tabData.GetLength(1); j++)
                {
                    tabData[i, j] = Data.Rows[i][AxisValues[j]].toDouble();
                }
            }

            double result = ClassificationQuality(neighboursCount, tabData);
            UpdateTable();
            UpdateClassComboBox();
            UpdateAllColumnValuesComboBoxes();

            MessageBox.Show(String.Format("Classification Quality equals: {0:0.0000}%", result));
        }

        private double ClassificationQuality(int neighboursCount, double[,] tabData)
        {
            int correctCount = 0;
            string columnName = SelectedClassificationColumn + "-" + SelectedClassificationMethod;
            if (!Data.Columns.Contains(columnName))
            {
                DataColumn newColumn = new DataColumn(columnName, typeof(string));
                Data.Columns.Add(newColumn);
                StringClassValues.Add(columnName);
                AllColumnsValues.Add(columnName);
                fileData.Classes.Add(Data.Columns.IndexOf(columnName), new List<string>());

            }
            for (int i = 0; i < Data.Rows.Count; i++)
            {
                string result = Classificate(neighboursCount, i, Data.Rows[i], tabData);
                Data.Rows[i][Data.Columns.IndexOf(columnName)] = result;
                if (result == Data.Rows[i][SelectedClassificationColumn].ToString()) correctCount++;
            }
            return (double)correctCount / (double)Data.Rows.Count;
        }

        private double ClassificationQualitySimple(int neighboursCount, double[,] tabData)
        {
            int correctCount = 0;
            for (int i = 0; i < Data.Rows.Count; i++)
            {
                string result = Classificate(neighboursCount, i, Data.Rows[i], tabData);
                if (result == Data.Rows[i][SelectedClassificationColumn].ToString()) correctCount++;
            }
            return (double)correctCount / (double)Data.Rows.Count;
        }

        private string Classificate(int neighboursCount, int excluded, DataRow row, double[,] tabData)
        {
            int object1ID = Data.Rows.IndexOf(row);

            SortedDictionary<double, List<int>> distances = new SortedDictionary<double, List<int>>();
            for (int i = 0; i < Data.Rows.Count; i++)
            {
                if (i == excluded) continue;
                int object2ID = i;
                double dist = Dist(SelectedClassificationMethod, object1ID, object2ID, tabData, excluded);
                if (distances.ContainsKey(dist)) distances[dist].Add(i);
                else distances.Add(dist, new List<int>() { i });

            }

            int[] neighbours = GetNeighbours(neighboursCount, distances);

            Dictionary<string, int> classCount = new Dictionary<string, int>();
            foreach (int index in neighbours)
            {
                string value = Data.Rows[index][SelectedClassificationColumn].ToString();
                if (classCount.ContainsKey(value)) classCount[value]++;
                else classCount.Add(value, 1);
            }
            return classCount.FirstOrDefault(x => x.Value == classCount.Values.Max()).Key;
        }

        private int[] GetNeighbours(int neighboursCount, SortedDictionary<double, List<int>> distances)
        {
            int[] neighbours = new int[neighboursCount];
            int count = 0;
            foreach (List<int> list in distances.Values)
            {
                for (int i = 0; i < list.Count; i++)
                {
                    if (count == neighboursCount - 1) return neighbours;
                    neighbours[count] = list[i];
                    count++;
                }
            }
            return neighbours;
        }
    }
}
