using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DecisionSupportSystem
{
    using Logic;
    using Microsoft.Research.DynamicDataDisplay.Common;
    using Model;
    using System.Data;
    using System.Windows;

    public partial class MainWindow
    {
        private List<string> _axisValues;
        private List<string> _classValues;
        private string _selectedX;
        private string _selectedY;
        private string _selectedClass;

        /// <summary>
        /// List of Numeric Columns
        /// </summary>
        public List<string> AxisValues
        {
            get
            {
                return _axisValues;
            }
            set
            {
                _axisValues = value;
                NotifyPropertyChanged("AxisValues");
            }
        }

        /// <summary>
        /// List of Class Columns
        /// </summary>
        public List<string> ClassValues
        {
            get
            {
                return _classValues;
            }
            set
            {
                _classValues = value;
                NotifyPropertyChanged("ClassValues");
            }
        }

        public string SelectedX
        {
            get
            {
                return _selectedX;
            }
            set
            {
                _selectedX = value;
                NotifyPropertyChanged("SelectedX");
            }
        }

        public string SelectedY
        {
            get
            {
                return _selectedY;
            }
            set
            {
                _selectedY = value;
                NotifyPropertyChanged("SelectedY");
            }
        }

        public string SelectedClass
        {
            get
            {
                return _selectedClass;
            }
            set
            {
                _selectedClass = value;
                NotifyPropertyChanged("SelectedClass");
            }
        }

        private void InitializeAxisComboBoxes()
        {
            List<string> axisValues = new List<string>();
            for (int i = 0; i < fileData.NumericalColumns.Count; i++)
            {
                int colIndex = fileData.NumericalColumns[i];
                string colName = Data.Columns[colIndex].ColumnName;
                axisValues.Add(colName);
            }

            AxisValues = axisValues;
            if (AxisValues.Count > 0) SelectedX = SelectedY = SelectedDiscr = SelectedClassificationColumn = SelectedSimilarityColumn1 = SelectedSimilarityColumn2 = SelectedRemovalColumn = AxisValues[0];
        }

        private void Chart2D_Click(object sender, RoutedEventArgs e)
        {
            Data.DefaultView.Sort = SelectedX + " asc";
            DataTable sorted = Data.DefaultView.ToTable();
            Data = null;
            Data = sorted;
            GraphModel model = new GraphModel();
            model.AxisNameX = SelectedX;
            model.AxisNameY = SelectedY;

            int idX = Data.Columns.IndexOf(SelectedX);
            int idY = Data.Columns.IndexOf(SelectedY);

            double minX = double.MaxValue;
            double maxX = double.MinValue;
            double minY = double.MaxValue;
            double maxY = double.MinValue;

            var points = new List<List<Point>>();

            // if there are classes
            if (ClassValues.Count > 0)
            {
                int numerizedClassId = Data.Columns.IndexOf(SelectedClass);
                int classId = Data.Columns.IndexOf(SelectedClass.Substring(0, SelectedClass.Length - 6));
                model.ClassNames = fileData.Classes[classId];

                for (int i = 0; i < model.ClassNames.Count; i++)
                {
                    points.Add(new List<Point>());
                }


                for (int i = 0; i < Data.Rows.Count; i++)
                {
                    double valueX = Data.Rows[i][idX].toDouble();
                    double valueY = Data.Rows[i][idY].toDouble();

                    if (valueX < minX) minX = valueX;
                    else if (valueX > maxX) maxX = valueX;

                    if (valueY < minY) minY = valueY;
                    else if (valueY > maxY) maxY = valueY;

                    int numerizedClassValue = Data.Rows[i][numerizedClassId].toInt() - 1;
                    points[numerizedClassValue].Add(new Point(valueX, valueY));
                }
            }
            else
            {
                // ignore classes
                points.Add(new List<Point>(Data.Rows.Count));

                for (int i = 0; i < Data.Rows.Count; i++)
                {
                    double valueX = Data.Rows[i][idX].toDouble();
                    double valueY = Data.Rows[i][idY].toDouble();

                    if (valueY < minY) minY = valueY;
                    else if (valueY >= maxY) maxY = valueY;

                    points[0].Add(new Point(valueX, valueY));
                }
            }
            List<RingArray<Point>> outputPoints = new List<RingArray<Point>>();
            for (int i = 0; i < points.Count; i++)
            {
                outputPoints.Add(new RingArray<Point>(points[i].Count));
                points[i].ForEach((x) => { outputPoints[i].Add(x); });
            }

            model.Points = outputPoints;
            model.MinValueX = (int)Math.Floor(minX) - 1;
            model.MinValueY = (int)Math.Floor(minY) - 1;
            model.MaxValueX = (int)Math.Ceiling(maxX) + 1;
            model.MaxValueY = (int)Math.Ceiling(maxY) + 1;

            var Window2D = new _2DChart(model, false);
            Window2D.Show();
        }
    }
}
