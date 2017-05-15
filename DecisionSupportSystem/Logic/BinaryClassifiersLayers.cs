using DecisionSupportSystem.Logic;
using DecisionSupportSystem.Model;
using Microsoft.Research.DynamicDataDisplay.Common;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace DecisionSupportSystem
{
    public partial class MainWindow
    {
        private List<List<int>> vectors;
        private List<bool> isPointIgnored;
        private GraphModel model;
        private bool isVisualizable = false;
        private List<CountResult> lines;
        private string lastClass = "";
        private double minX, maxX, minY, maxY;

        private void ProcessBinary_Click(object sender, RoutedEventArgs e)
        {
            ProcessBinaryInit();
        }

        private void ProcessBinaryInit()
        {
            List<Entry>[] entries = new List<Entry>[Data.Columns.Count - 1];

            for (int i = 0; i < Data.Columns.Count - 1; i++)
            {
                entries[i] = new List<Entry>(Data.Rows.Count);
                for (int j = 0; j < Data.Rows.Count; j++)
                {
                    Entry entry = new Entry();
                    entry.Index = j;
                    entry.Value = Data.Rows[j][i].toDouble();
                    entry.Class = Data.Rows[j][Data.Columns.Count - 1].ToString();
                    entries[i].Add(entry);
                }
                entries[i] = entries[i].OrderBy(x => x.Value).ToList();
            }

            isVisualizable = false;
            if (Data.Columns.Count == 3)
            {
                isVisualizable = true;
            }

            if (isVisualizable)
            {
                model = new GraphModel();
                model.AxisNameX = Data.Columns[0].ColumnName;
                model.AxisNameY = Data.Columns[1].ColumnName;
                model.ClassNames = entries[0].Select(x => x.Class).Distinct().ToList();
                model.CutsStart = model.ClassNames.Count;

                var points = new List<List<Point>>();

                // if there are classes
                if (model.ClassNames.Count > 0)
                {
                    for (int i = 0; i < model.ClassNames.Count; i++)
                    {
                        points.Add(new List<Point>());
                    }

                    for (int i = 0; i < Data.Rows.Count; i++)
                    {
                        double valueX = Data.Rows[i][0].toDouble();
                        double valueY = Data.Rows[i][1].toDouble();

                        if (valueX < minX) minX = valueX;
                        else if (valueX > maxX) maxX = valueX;

                        if (valueY < minY) minY = valueY;
                        else if (valueY > maxY) maxY = valueY;

                        points[model.ClassNames.IndexOf(Data.Rows[i][Data.Columns.Count - 1].ToString())].Add(new Point(valueX, valueY));
                    }
                }

                List<RingArray<Point>> outputPoints = new List<RingArray<Point>>();
                for (int i = 0; i < points.Count; i++)
                {
                    outputPoints.Add(new RingArray<Point>(points[i].Count));
                    points[i].ForEach((x) => { outputPoints[i].Add(x); });
                }

                model.Points = outputPoints;
            }

            lines = new List<CountResult>();
            vectors = new List<List<int>>(Data.Rows.Count);
            isPointIgnored = new List<bool>(Data.Rows.Count);
            for (int i = 0; i < Data.Rows.Count; i++)
            {
                vectors.Add(new List<int>());
                isPointIgnored.Add(false);
            }

            ProcessBinary(entries);

            if (isVisualizable)
            {
                int count = model.Points.Count - model.ClassNames.Count;
                for (int i = 0; i < count; i++)
                {
                    model.ClassNames.Add("Cut " + (i + 1));
                }
            }
        }

        private void ProcessBinary(List<Entry>[] entries)
        {
            int rowCount = entries[0].Count;
            if (rowCount == 0)
            {
                MessageBox.Show("Finished. No more entries.");
                return;
            }

            List<CountResult> results = new List<CountResult>();
            List<CountResult> resultsMixed = new List<CountResult>();
            GatherCountData(entries, rowCount, results, resultsMixed);

            if (results.Count == 0)
            {
                if (resultsMixed.Count != 0)
                {
                    EvaluateEntries(entries, rowCount, resultsMixed);
                }
                else
                {
                    MessageBox.Show("Finished. No more results.");
                    return;
                }
            }
            else
            {
                EvaluateEntries(entries, rowCount, results);
            }
        }

        private void GatherCountData(List<Entry>[] entries, int rowCount, List<CountResult> results, List<CountResult> resultsMixed)
        {
            List<IGrouping<double, Entry>>[] grouped = new List<IGrouping<double, Entry>>[entries.Length];
            int maxLength = 0;
            for (int i = 0; i < entries.Length; i++)
            {
                grouped[i] = entries[i].GroupBy(x => x.Value).ToList();
                if (grouped[i].Count > maxLength)
                {
                    maxLength = grouped[i].Count;
                }
            }
            if (maxLength <= 1)
            {
                var classes = entries[0].GroupBy(x => x.Class).ToList();
                int max = 0;
                for (int i = 0; i < classes.Count; i++)
                {
                    int cnt = classes[i].Count();
                    if (cnt > max)
                    {
                        max = cnt;
                        lastClass = classes[i].Key;
                    }
                }
                for (int i = 0; i < entries[0].Count; i++)
                {
                    if (entries[0][i].Class != lastClass)
                    {
                        isPointIgnored[entries[0][i].Index] = true;
                    }
                }
                return;
            }

            for (int i = 0; i < entries.Length; i++)
            {
                int length = grouped[i].Count();

                //ascending
                string curClass = entries[i][0].Class;
                int count = 0;
                for (int j = 0; j < length; j++)
                {
                    if (grouped[i][j].All(x => x.Class == curClass)) count += grouped[i][j].Count();
                    else break;
                }

                if (count == entries[0].Count) lastClass = curClass;
                else
                {
                    if (count != 0)
                    {
                        CountResult tmp = new CountResult() { Column = i, Count = count, IsAscending = true, Class = curClass };
                        results.Add(tmp);
                    }
                    else
                    {
                        count = 0;
                        var classes = grouped[i][0].GroupBy(x => x.Class).ToList();
                        for (int j = 0; j < classes.Count; j++)
                        {
                            if (classes[j].Count() > count)
                            {
                                count = classes[j].Count();
                                curClass = classes[j].Key;
                            }
                        }

                        if (count == entries[0].Count) lastClass = curClass;
                        if (count != 0)
                        {
                            for (int j = 1; j < length; j++)
                            {
                                if (grouped[i][j].All(x => x.Class == curClass)) count += grouped[i][j].Count();
                                else break;
                            }

                            if (count != entries[0].Count)
                            {
                                CountResult tmp = new CountResult() { Column = i, Count = count, IsAscending = true, Class = curClass };
                                resultsMixed.Add(tmp);
                            }
                        }
                    }
                }

                //descending
                curClass = entries[i][rowCount - 1].Class;
                count = 0;
                for (int j = length - 1; j >= 0; j--)
                {
                    if (grouped[i][j].All(x => x.Class == curClass)) count += grouped[i][j].Count();
                    else break;
                }
                if (count == entries[0].Count) lastClass = curClass;
                else
                {
                    if (count != 0)
                    {
                        CountResult tmp = new CountResult() { Column = i, Count = count, IsAscending = false, Class = curClass };
                        results.Add(tmp);
                    }
                    else
                    {
                        count = 0;
                        var classes = grouped[i][length - 1].GroupBy(x => x.Class).ToList();
                        for (int j = 0; j < classes.Count; j++)
                        {
                            if (classes[j].Count() > count)
                            {
                                count = classes[j].Count();
                                curClass = classes[j].Key;
                            }
                        }

                        if (count == entries[0].Count) lastClass = curClass;
                        if (count != 0)
                        {
                            for (int j = length - 2; j >= 0; j--)
                            {
                                if (grouped[i][j].All(x => x.Class == curClass)) count += grouped[i][j].Count();
                                else break;
                            }

                            if (count != entries[0].Count)
                            {
                                CountResult tmp = new CountResult() { Column = i, Count = count, IsAscending = false, Class = curClass };
                                resultsMixed.Add(tmp);
                            }
                        }
                    }
                }
            };
        }

        private void EvaluateEntries(List<Entry>[] entries, int rowCount, List<CountResult> results)
        {
            List<int> toRemove = new List<int>();
            CountResult maxCountResult = results[results.MaxResultIndex()];
            int cmpCol = maxCountResult.Column;
            int countDown = maxCountResult.Count;
            string resClass = maxCountResult.Class;

            if (maxCountResult.IsAscending)
            {
                if (countDown < entries[cmpCol].Count)
                {
                    Entry nextEntry = entries[cmpCol][countDown];
                    maxCountResult.MaxValue = (entries[cmpCol][countDown - 1].Value + nextEntry.Value) / 2;
                }
                else
                {
                    maxCountResult.MaxValue = entries[cmpCol][countDown - 1].Value;
                }

            }
            else
            {
                if (rowCount - countDown - 1 >= 0)
                {
                    Entry prevEntry = entries[cmpCol][rowCount - countDown - 1];
                    maxCountResult.MaxValue = (entries[cmpCol][rowCount - countDown].Value + prevEntry.Value) / 2;
                }
                else
                {
                    maxCountResult.MaxValue = entries[cmpCol][rowCount - countDown].Value;
                }
            }

            List<Entry> comparedEntries = entries[cmpCol];
            lines.Add(maxCountResult);
            if (isVisualizable)
            {
                model.Points.Add(new RingArray<Point>(4));
                if (maxCountResult.Column == 0)
                {
                    if (maxCountResult.IsAscending) minX = maxCountResult.MaxValue;
                    else maxX = maxCountResult.MaxValue;
                    double diff = Math.Abs(maxY - minY) / 40;
                    double margin = 0;
                    if (maxCountResult.IsAscending)
                    {
                        model.Points[model.Points.Count - 1].Add(new Point(maxCountResult.MaxValue + margin - diff, minY));
                        model.Points[model.Points.Count - 1].Add(new Point(maxCountResult.MaxValue + margin, minY));
                        model.Points[model.Points.Count - 1].Add(new Point(maxCountResult.MaxValue + margin, maxY));
                        model.Points[model.Points.Count - 1].Add(new Point(maxCountResult.MaxValue + margin - diff, maxY));
                    }
                    else
                    {
                        model.Points[model.Points.Count - 1].Add(new Point(maxCountResult.MaxValue - margin + diff, minY));
                        model.Points[model.Points.Count - 1].Add(new Point(maxCountResult.MaxValue - margin, minY));
                        model.Points[model.Points.Count - 1].Add(new Point(maxCountResult.MaxValue - margin, maxY));
                        model.Points[model.Points.Count - 1].Add(new Point(maxCountResult.MaxValue - margin + diff, maxY));
                    }
                }
                else
                {
                    if (maxCountResult.IsAscending) minY = maxCountResult.MaxValue;
                    else maxY = maxCountResult.MaxValue;
                    double diff = Math.Abs(maxX - minX) / 40;
                    double margin = 0;
                    if (maxCountResult.IsAscending)
                    {
                        model.Points[model.Points.Count - 1].Add(new Point(minX, maxCountResult.MaxValue + margin - diff));
                        model.Points[model.Points.Count - 1].Add(new Point(minX, maxCountResult.MaxValue + margin));
                        model.Points[model.Points.Count - 1].Add(new Point(maxX, maxCountResult.MaxValue + margin));
                        model.Points[model.Points.Count - 1].Add(new Point(maxX, maxCountResult.MaxValue + margin - diff));
                    }
                    else
                    {
                        model.Points[model.Points.Count - 1].Add(new Point(minX, maxCountResult.MaxValue - margin + diff));
                        model.Points[model.Points.Count - 1].Add(new Point(minX, maxCountResult.MaxValue - margin));
                        model.Points[model.Points.Count - 1].Add(new Point(maxX, maxCountResult.MaxValue - margin));
                        model.Points[model.Points.Count - 1].Add(new Point(maxX, maxCountResult.MaxValue - margin + diff));
                    }
                }
            }

            for (int i = 0; i < vectors.Count; i++)
            {
                vectors[i].Add(0);
            }

            if (maxCountResult.IsAscending)
            {
                for (int i = 0; i < countDown; i++)
                {
                    int id = comparedEntries[i].Index;
                    vectors[id][vectors[id].Count - 1] = 1;
                    toRemove.Add(id);
                }
            }
            else
            {
                for (int i = comparedEntries.Count - 1; i >= comparedEntries.Count - countDown; i--)
                {
                    int id = comparedEntries[i].Index;
                    vectors[id][vectors[id].Count - 1] = 1;
                    toRemove.Add(id);
                }
            }

            // mark points to be ignored
            for (int i = 0; i < toRemove.Count; i++)
            {
                int id = toRemove[i];
                if (resClass != Data.Rows[id][Data.Columns.Count - 1].ToString())
                {
                    isPointIgnored[id] = true;
                }
            }

            for (int j = 0; j < entries.Length; j++)
            {
                entries[j].RemoveAll(x => toRemove.Contains(x.Index));
            }

            ProcessBinary(entries);
        }

        private void ClassificateBinary_Click(object sender, RoutedEventArgs e)
        {
            List<int> vector = new List<int>();
            int lastRow = Data.Rows.Count - 1;
            DataRow row = Data.Rows[lastRow];

            bool classified = false;
            for (int i = 0; i < lines.Count; i++)
            {
                if (!classified)
                {
                    CountResult line = lines[i];
                    if (line.IsAscending && row[line.Column].toDouble() <= line.MaxValue)
                    {
                        vector.Add(1);
                        row[Data.Columns.Count - 1] = line.Class;
                        classified = true;

                    }
                    else if (!line.IsAscending && row[line.Column].toDouble() >= line.MaxValue)
                    {
                        vector.Add(1);
                        row[Data.Columns.Count - 1] = line.Class;
                        classified = true;
                    }
                    else
                    {
                        vector.Add(0);
                    }
                }
                else
                {
                    vector.Add(0);
                }
            }

            if (!classified)
            {
                row[Data.Columns.Count - 1] = lastClass;
            }
            MessageBox.Show("Classified as: " + row[Data.Columns.Count - 1]);
            vectors.Add(vector);
            isPointIgnored.Add(false);
        }

        private void VisualizeBinary_Click(object sender, RoutedEventArgs e)
        {
            if (isVisualizable)
            {
                var Window2D = new _2DChart(model, true);
                Window2D.Show();
            }
        }

        private void SaveBinary_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog fileDialog = new SaveFileDialog();
            fileDialog.Filter = "Text file (*.txt)|*.txt|CSV file(*.csv)|*.csv|All files (*.*)|*.*";
            fileDialog.FilterIndex = 2;
            fileDialog.DefaultExt = ".csv";
            fileDialog.FileName = "values.csv";
            var result = fileDialog.ShowDialog();
            if (result.HasValue && result.Value)
            {
                InputOutput.SaveFileVectors(fileDialog.FileName, Data, vectors, isPointIgnored);
                MessageBox.Show("File " + fileDialog.FileName + " saved.");
            }
            else
            {
                MessageBox.Show("Error saving file");
            }
        }

        private void SaveBinaryIgnored_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog fileDialog = new SaveFileDialog();
            fileDialog.Filter = "Text file (*.txt)|*.txt|CSV file(*.csv)|*.csv|All files (*.*)|*.*";
            fileDialog.FilterIndex = 2;
            fileDialog.DefaultExt = ".csv";
            fileDialog.FileName = "ignored.csv";
            var result = fileDialog.ShowDialog();
            if (result.HasValue && result.Value)
            {
                InputOutput.SaveFileVectorsIgnored(fileDialog.FileName, Data, vectors, isPointIgnored);
                MessageBox.Show("File " + fileDialog.FileName + " saved.");
            }
            else
            {
                MessageBox.Show("Error saving file");
            }
        }

    }
}
