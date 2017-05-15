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
        private List<string> _decisionTreeMethods;
        private string _selectedDecisionTreeMethod;
        int attrCt;
        int[] pickedAttr;
        List<string> classSet;

        public List<string> DecisionTreeMethods
        {
            get
            {
                return _decisionTreeMethods;
            }
            set
            {
                _decisionTreeMethods = value;
                NotifyPropertyChanged("DecisionTreeMethods");
            }
        }

        public string SelectedDecisionTreeMethod
        {
            get
            {
                return _selectedDecisionTreeMethod;
            }
            set
            {
                _selectedDecisionTreeMethod = value;
                NotifyPropertyChanged("SelectedDecisionTreeMethod");
            }
        }

        private void InitializeDecisionTreeMethodValues()
        {
            _decisionTreeMethods = new List<string>()
            {
                "Random",
                "Entropy"
            };
            SelectedDecisionTreeMethod = DecisionTreeMethods[0];
        }

        private void CheckTreeClassificationQuality_Click(object sender, RoutedEventArgs e)
        {
            int discretizationCount;
            if (!int.TryParse(TreeDiscretizationTextBox.Text, out discretizationCount))
            {
                MessageBox.Show("Wrong Discretization Count!");
                return;
            }

            int[,] tabData = new int[Data.Rows.Count, AxisValues.Count];
            for (int i = 0; i < tabData.GetLength(0); i++)
            {
                for (int j = 0; j < tabData.GetLength(1); j++)
                {
                    tabData[i, j] = Data.Rows[i][AxisValues[j]].toInt();
                }
            }

            double result = TreeClassificationQuality(tabData, discretizationCount);
            UpdateTable();
            UpdateClassComboBox();
            UpdateAllColumnValuesComboBoxes();

            MessageBox.Show(String.Format(SelectedDecisionTreeMethod + " Tree Classification Quality equals: {0:0.0000}%", result));
        }

        private double TreeClassificationQuality(int[,] tabData, int discretizationCount)
        {
            int correctCount = 0;
            string columnName = SelectedClassificationColumn + "-" + "Tree " + SelectedDecisionTreeMethod;
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
                string result = TreeClassificate(tabData, discretizationCount, i);
                Data.Rows[i][Data.Columns.IndexOf(columnName)] = result;
                if (result == Data.Rows[i][SelectedClassificationColumn].ToString()) correctCount++;
            }
            return (double)correctCount / (double)Data.Rows.Count;
        }

        private string TreeClassificate(int[,] tabData, int discretizationCount, int excluded)
        {
            TreeNode root = BuildTree(tabData, discretizationCount, excluded);
            return TreeClassificateObject(tabData, excluded, root);
        }

        private TreeNode BuildTree(int[,] tabData, int discretizationCount, int excluded)
        {
            TreeNode root = null;
            switch (SelectedDecisionTreeMethod)
            {
                case "Random":
                    root = BuildTreeRandomAttrInit(tabData, discretizationCount, excluded);
                    break;
                case "Entropy":
                    root = BuildTreeEntropyAttrInit(tabData, discretizationCount, excluded);
                    break;

            }
            return root;
        }

        private TreeNode BuildTreeRandomAttrInit(int[,] tabData, int discretizationCount, int excluded)
        {
            attrCt = tabData.GetLength(1);
            Random random = new Random();
            List<int> tmp = new List<int>();
            pickedAttr = new int[attrCt];
            for (int i = 0; i < attrCt; i++)
            {
                tmp.Add(i);
            }
            int selected;

            for (int i = 0; i < attrCt; i++)
            {
                selected = random.Next(attrCt - i);
                pickedAttr[i] = tmp[selected];
                tmp.RemoveAt(selected);
            }
            TreeNode root = new TreeNode(false, -1, -1);
            BuildTreeRandomAttr(root, 0, discretizationCount);

            string objectClass;
            for (int i = 0; i < tabData.GetLength(0); i++)
            {
                if (i == excluded) continue;
                objectClass = Data.Rows[i][SelectedClassificationColumn].ToString();
                BuildTreePropagateObject(root, i, tabData, objectClass);
            }
            return root;
        }

        private void BuildTreePropagateObject(TreeNode node, int objectID, int[,] tabData, string objectClass)
        {
            for (int i = 0; i < node.Children.Count; i++)
            {
                TreeNode curNode = node.Children[i];
                if (tabData[objectID, curNode.Attribute] == curNode.Value)
                {
                    if (curNode.ClassCountLeaf.ContainsKey(objectClass)) curNode.ClassCountLeaf[objectClass]++;
                    else curNode.ClassCountLeaf.Add(objectClass, 1);
                    if (curNode.IsLeaf)
                    {
                        curNode.Class = curNode.ClassCountLeaf.Aggregate((l, r) => l.Value > r.Value ? l : r).Key;
                    }
                    else
                    {
                        BuildTreePropagateObject(curNode, objectID, tabData, objectClass);
                    }
                    return;
                }
            }
        }

        private void BuildTreeRandomAttr(TreeNode node, int depth, int discretizationCount)
        {
            bool isLeaf = false;
            if (depth == attrCt - 1) isLeaf = true;
            for (int i = 0; i < discretizationCount; i++)
            {
                TreeNode child = new TreeNode(isLeaf, pickedAttr[depth], i + 1);
                node.Children.Add(child);
                if (!isLeaf) BuildTreeRandomAttr(child, depth + 1, discretizationCount);
            }
        }

        private TreeNode BuildTreeEntropyAttrInit(int[,] tabData, int discretizationCount, int excluded)
        {
            if (classSet == null)
            {
                classSet = new List<string>();
                HashSet<string> hashSet = new HashSet<string>();
                for (int i = 0; i < tabData.GetLength(0); i++)
                {
                    if (i != excluded)
                    {
                        hashSet.Add(Data.Rows[i][SelectedClassificationColumn].ToString());
                    }
                }

                for (int i = 0; i < hashSet.Count; i++)
                {
                    classSet.Add(hashSet.ElementAt(i));
                }
            }

            List<int> dataSet = new List<int>();
            for (int i = 0; i < tabData.GetLength(0); i++)
            {
                if (i != excluded)
                {
                    dataSet.Add(i);
                }
            }

            List<int> attrSet = new List<int>();
            for (int i = 0; i < tabData.GetLength(1); i++)
            {
                attrSet.Add(i);
            }

            TreeNode root = new TreeNode(false, -1, -1);
            BuildTreeEntropyAttr(root, discretizationCount, tabData, dataSet, attrSet);

            return root;
        }

        private void BuildTreeEntropyAttr(TreeNode node, int discretizationCount, int[,] tabData, List<int> dataSet, List<int> attrSet)
        {
            if (attrSet.Count == 0) return;
            bool isLeaf = false;
            if (attrSet.Count == 1) isLeaf = true;

            List<int[,]> SumTables = new List<int[,]>();
            for (int i = 0; i < attrSet.Count; i++)
            {
                SumTables.Add(new int[discretizationCount, classSet.Count + 1]);
            }

            for (int i = 0; i < dataSet.Count; i++)
            {
                for (int j = 0; j < attrSet.Count; j++)
                {
                    int value = tabData[dataSet[i], attrSet[j]];
                    int classID = classSet.IndexOf(Data.Rows[dataSet[i]][SelectedClassificationColumn].ToString());
                    SumTables[j][value - 1, classID]++;
                    SumTables[j][value - 1, classSet.Count]++;
                }
            }

            double[] attrEntr = new double[attrSet.Count];
            for (int i = 0; i < attrSet.Count; i++)
            {
                double value = 0;
                for (int j = 0; j < discretizationCount; j++)
                {
                    int[] parameters = new int[classSet.Count];
                    int discElemCount = SumTables[i][j, classSet.Count];
                    for (int k = 0; k < classSet.Count; k++)
                    {
                        parameters[k] = SumTables[i][j, k];
                    }
                    value += Entropy(parameters, discElemCount) * discElemCount / dataSet.Count;
                }
                attrEntr[i] = value;
            }
            int bestAttrID = attrEntr.IndexOf(attrEntr.Min());
            int bestAttr = attrSet[bestAttrID];

            List<int> newAttrSet = new List<int>(attrSet);
            newAttrSet.Remove(bestAttr);

            for (int i = 0; i < discretizationCount; i++)
            {
                if (SumTables[bestAttrID][i, classSet.Count] == 0)
                {
                    continue;
                }

                TreeNode child = new TreeNode(true, bestAttr, i + 1);
                node.Children.Add(child);
                bool singleClass = false;
                int max = 0;
                for (int j = 0; j < classSet.Count; j++)
                {
                    var value = SumTables[bestAttrID][i, j];
                    child.ClassCountLeaf.Add(classSet[j], value);

                    // if all values are one class
                    if (value != 0 && value == SumTables[bestAttrID][i, classSet.Count])
                    {
                        child.Class = classSet[j];
                        singleClass = true;
                    }
                    else
                    {
                        if (value > max)
                        {
                            max = value;
                            child.Class = classSet[j];
                        }
                    }
                }
                if (!singleClass && !isLeaf)
                {
                    child.Class = "";
                    child.IsLeaf = false;
                    List<int> newDataSet = dataSet.Where((x) => tabData[x, bestAttr] == i + 1).ToList();
                    BuildTreeEntropyAttr(child, discretizationCount, tabData, newDataSet, new List<int>(newAttrSet));
                }
            }
        }

        private double Entropy(int[] parameters, int elemCount)
        {
            double value = 0;
            for (int i = 0; i < parameters.Length; i++)
            {
                double prob = (double)parameters[i] / elemCount;
                if (prob > 0.001)
                {
                    value -= (prob) * Math.Log(prob, 2);
                }
            }
            return value;
        }

        private string TreeClassificateObject(int[,] tabData, int objectID, TreeNode node)
        {
            for (int i = 0; i < node.Children.Count; i++)
            {
                TreeNode curNode = node.Children[i];
                if (tabData[objectID, curNode.Attribute] == curNode.Value)
                {
                    if (curNode.IsLeaf)
                    {
                        return curNode.Class;
                    }
                    else
                    {
                        return TreeClassificateObject(tabData, objectID, curNode);
                    }
                }
            }
            return "";
        }
    }
}
