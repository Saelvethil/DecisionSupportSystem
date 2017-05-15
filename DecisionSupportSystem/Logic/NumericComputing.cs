using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace DecisionSupportSystem
{
    using Logic;
    public partial class MainWindow
    {
        private List<string> _stringClassValues;
        private string _selectedStringClassValue;
        private string _selectedDiscr;

        public List<string> StringClassValues
        {
            get
            {
                return _stringClassValues;
            }
            set
            {
                _stringClassValues = value;
                NotifyPropertyChanged("StringClassValues");
            }
        }

        public string SelectedStringClassValue
        {
            get
            {
                return _selectedStringClassValue;
            }
            set
            {
                _selectedStringClassValue = value;
                NotifyPropertyChanged("SelectedStringClassValue");
            }
        }

        public string SelectedDiscr
        {
            get
            {
                return _selectedDiscr;
            }
            set
            {
                _selectedDiscr = value;
                NotifyPropertyChanged("SelectedDiscr");
            }
        }

        private void InitializeStringClassComboBox()
        {
            List<string> classValues = new List<string>();
            foreach (int colIndex in fileData.Classes.Keys)
            {
                string colName = Data.Columns[colIndex].ColumnName;
                classValues.Add(colName);
            }
            StringClassValues = classValues;
            ClassValues = classValues;
            if (StringClassValues.Count > 0) SelectedStringClassValue = StringClassValues[0];
        }

        private void UpdateClassComboBox()
        {
            var tmp = ClassValues;
            ClassValues = null;
            ClassValues = tmp;
            if (ClassValues.Count > 0) SelectedClass = ClassValues[0];

            var tmp2 = StringClassValues;
            StringClassValues = null;
            StringClassValues = tmp2;
            if (StringClassValues.Count > 0) SelectedStringClassValue = StringClassValues[0];
        }

        private void ConvertClassButton_Click(object sender, RoutedEventArgs e)
        {
            string columnName = SelectedStringClassValue + " Class";

            if (Data.Columns.Contains(columnName)) return;
            DataColumn newColumn = new DataColumn(columnName, typeof(int));
            Data.Columns.Add(newColumn);
            ClassValues.Add(columnName);
            AxisValues.Add(columnName);
            AllColumnsValues.Add(columnName);

            int colID = Data.Columns.IndexOf(SelectedStringClassValue);
            double count = Data.Rows.Count;
            for (int j = 0; j < count; j++)
            {
                string value = Data.Rows[j][colID].ToString();
                int index = fileData.Classes[colID].IndexOf(value);
                // if class value is new we add it
                if (index == -1)
                {
                    fileData.Classes[colID].Add(value);
                    index = fileData.Classes[colID].Count - 1;
                }
                Data.Rows[j][columnName] = index + 1;
            }

            UpdateTable();
            UpdateAxisComboBoxes();
            UpdateClassComboBox();
            UpdateAllColumnValuesComboBoxes();
        }

        private void ConvertAllButton_Click(object sender, RoutedEventArgs e)
        {
            foreach(string ClassValue in StringClassValues)
            {
                string columnName = ClassValue + " Class";

                if (Data.Columns.Contains(columnName)) return;
                DataColumn newColumn = new DataColumn(columnName, typeof(int));
                Data.Columns.Add(newColumn);
                ClassValues.Add(columnName);
                AxisValues.Add(columnName);
                AllColumnsValues.Add(columnName);

                int colID = Data.Columns.IndexOf(ClassValue);
                double count = Data.Rows.Count;
                for (int j = 0; j < count; j++)
                {
                    string value = Data.Rows[j][colID].ToString();
                    int index = fileData.Classes[colID].IndexOf(value);
                    // if class value is new we add it
                    if (index == -1)
                    {
                        fileData.Classes[colID].Add(value);
                        index = fileData.Classes[colID].Count - 1;
                    }
                    Data.Rows[j][columnName] = index + 1;
                }
            }

            UpdateTable();
            UpdateAxisComboBoxes();
            UpdateClassComboBox();
            UpdateAllColumnValuesComboBoxes();
        }

        private void DiscretizeButton_Click(object sender, RoutedEventArgs e)
        {
            int count;
            if (int.TryParse(DiscretizeTextBox.Text, out count))
            {
                int colID = Data.Columns.IndexOf(SelectedDiscr);
                string newColumnName = SelectedDiscr + " Discr intv-" + count;
                if (Data.Columns.Contains(newColumnName)) return;
                DataColumn newColumn = new DataColumn(newColumnName, typeof(int));
                Data.Columns.Add(newColumn);
                double min = int.MaxValue;
                double max = int.MinValue;
                for (int j = 0; j < Data.Rows.Count; j++)
                {
                    double value = Data.Rows[j][colID].toDouble();
                    if (value < min) min = value;
                    else if (value > max) max = value;
                }
                double[] thresholds = new double[count];
                thresholds[0] = min;
                double interval = (max - min) / count;

                for (int ct = 1; ct < count; ct++)
                {
                    thresholds[ct] = thresholds[ct - 1] + interval;
                }

                for (int k = 0; k < Data.Rows.Count; k++)
                {
                    double value = Data.Rows[k][colID].toDouble();
                    for (int ct = count - 1; ct >= 0; ct--)
                    {
                        // <x1, x2) left closed, right open x1 <= x < x2
                        if (value >= thresholds[ct])
                        {
                            Data.Rows[k][newColumnName] = ct + 1;
                            break;
                        }
                    }
                }
                AxisValues.Add(newColumnName);
                AllColumnsValues.Add(newColumnName);
                UpdateTable();
                UpdateAxisComboBoxes();
                UpdateAllColumnValuesComboBoxes();
            }
            else MessageBox.Show("Enter valid number");
        }

        private void DiscretizeAllButton_Click(object sender, RoutedEventArgs e)
        {
            string[] newColumns = new string[fileData.NumericalColumns.Count];
            int count;
            if (int.TryParse(DiscretizeTextBox.Text, out count))
            {
                for (int i = 0; i < fileData.NumericalColumns.Count; i++)
                {
                    string newColumnName = Data.Columns[fileData.NumericalColumns[i]].ColumnName + " Discr intv-" + count;
                    if (Data.Columns.Contains(newColumnName)) return;
                    DataColumn newColumn = new DataColumn(newColumnName, typeof(int));
                    Data.Columns.Add(newColumn);
                    newColumns[i] = newColumnName;
                    double min = int.MaxValue;
                    double max = int.MinValue;
                    for (int j = 0; j < Data.Rows.Count; j++)
                    {
                        double value = Data.Rows[j][fileData.NumericalColumns[i]].toDouble();
                        if (value < min) min = value;
                        else if (value > max) max = value;
                    }
                    double[] thresholds = new double[count];
                    thresholds[0] = min;
                    double interval = (max - min) / count;

                    for (int ct = 1; ct < count; ct++)
                    {
                        thresholds[ct] = thresholds[ct - 1] + interval;
                    }

                    for (int k = 0; k < Data.Rows.Count; k++)
                    {
                        double value = Data.Rows[k][fileData.NumericalColumns[i]].toDouble();
                        for (int ct = count - 1; ct >= 0; ct--)
                        {
                            // <x1, x2) left closed, right open x1 <= x < x2
                            if (value >= thresholds[ct])
                            {
                                Data.Rows[k][newColumnName] = ct + 1;
                                break;
                            }
                        }
                    }
                }

                for (int i = 0; i < newColumns.Length; i++)
                {
                    AxisValues.Add(newColumns[i]);
                    AllColumnsValues.Add(newColumns[i]);
                }

                UpdateTable();
                UpdateAxisComboBoxes();
                UpdateAllColumnValuesComboBoxes();
            }
            else MessageBox.Show("Enter valid number");
        }

        private void NormalizeButton_Click(object sender, RoutedEventArgs e)
        {
            string[] newColumns = new string[fileData.NumericalColumns.Count];
            int colID = Data.Columns.IndexOf(SelectedDiscr);
            string newColumnName = SelectedDiscr + " Normalized";
            if (Data.Columns.Contains(newColumnName)) return;
            DataColumn newColumn = new DataColumn(newColumnName, typeof(double));
            Data.Columns.Add(newColumn);
            double sum = 0;
            double count = Data.Rows.Count;
            for (int j = 0; j < count; j++)
            {
                double value = Data.Rows[j][colID].toDouble();
                sum += value;
            }

            double average = sum / count;
            double stdSum = 0;

            for (int j = 0; j < count; j++)
            {
                double value = Data.Rows[j][colID].toDouble();
                stdSum += Math.Pow((value - average), 2);
            }
            double stdDev = Math.Sqrt(stdSum / (count - 1));

            for (int j = 0; j < count; j++)
            {
                double value = Data.Rows[j][colID].toDouble();
                double newValue = (value - average) / stdDev;
                Data.Rows[j][newColumnName] = newValue;
            }
            AxisValues.Add(newColumnName);
            AllColumnsValues.Add(newColumnName);
            UpdateTable();
            UpdateAxisComboBoxes();
            UpdateAllColumnValuesComboBoxes();
        }
        private void NormalizeAllButton_Click(object sender, RoutedEventArgs e)
        {
            string[] newColumns = new string[fileData.NumericalColumns.Count];

            for (int i = 0; i < fileData.NumericalColumns.Count; i++)
            {
                string newColumnName = Data.Columns[fileData.NumericalColumns[i]].ColumnName + " Normalized";
                if (Data.Columns.Contains(newColumnName)) return;
                DataColumn newColumn = new DataColumn(newColumnName, typeof(int));
                Data.Columns.Add(newColumn);
                newColumns[i] = newColumnName;
                double sum = 0;
                double count = Data.Rows.Count;
                for (int j = 0; j < count; j++)
                {
                    double value = Data.Rows[j][fileData.NumericalColumns[i]].toDouble();
                    sum += value;
                }

                double average = sum / count;
                double stdSum = 0;

                for (int j = 0; j < count; j++)
                {
                    double value = Data.Rows[j][fileData.NumericalColumns[i]].toDouble();
                    stdSum += Math.Pow((value - average), 2);
                }
                double stdDev = Math.Sqrt(stdSum / (count - 1));

                for (int j = 0; j < count; j++)
                {
                    double value = Data.Rows[j][fileData.NumericalColumns[i]].toDouble();
                    double newValue = (value - average) / stdDev;
                    Data.Rows[j][newColumnName] = newValue;
                }
            }

            for (int i = 0; i < newColumns.Length; i++)
            {
                AxisValues.Add(newColumns[i]);
                AllColumnsValues.Add(newColumns[i]);
            }

            UpdateTable();
            UpdateAxisComboBoxes();
            UpdateAllColumnValuesComboBoxes();
        }
    }
}



