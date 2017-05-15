using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DecisionSupportSystem
{
    using Logic;
    using Microsoft.Research.DynamicDataDisplay.Common;
    using Model;
    using System.Collections;
    using System.ComponentModel;
    using System.Data;
    using System.Runtime.CompilerServices;

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private DataTable _data;
        private ProcessedFile fileData;
        private string _selectedRemovalColumn;

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public DataTable Data
        {
            get
            {
                return _data;
            }
            set
            {
                _data = value;
                NotifyPropertyChanged("Data");
            }
        }

        public string SelectedRemovalColumn
        {
            get
            {
                return _selectedRemovalColumn;
            }
            set
            {
                _selectedRemovalColumn = value;
                NotifyPropertyChanged("SelectedRemovalColumn");
            }
        }

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
            this.WindowState = WindowState.Maximized;

            Data = new DataTable();
            InitializeClassificationMethodValues();
            InitializeDecisionTreeMethodValues();
            InitializeSimilarityMethodValues();
            TreeDiscretizationTextBox.Text = "5";
            DiscretizeTextBox.Text = "5";
            ClassificationNeighbourCount.Text = "5";
            ClusteringClassCount.Text = "5";
            IterationsCount.Text = "5";
        }

        private void UpdateAxisComboBoxes()
        {
            string selx = SelectedX;
            string sely = SelectedY;
            string selc = SelectedClass;
            string seld = SelectedDiscr;
            string selcc = SelectedClassificationColumn;
            string selsc1 = SelectedSimilarityColumn1;
            string selsc2 = SelectedSimilarityColumn2;
            string selrem = SelectedRemovalColumn;
            var temp = AxisValues;
            AxisValues = null;
            AxisValues = temp;
            SelectedX = selx;
            SelectedY = sely;
            SelectedClass = selc;
            SelectedDiscr = seld;
            SelectedClassificationColumn = selcc;
            SelectedSimilarityColumn1 = selsc1;
            SelectedSimilarityColumn2 = selsc2;
            SelectedRemovalColumn = selrem;
        }

        private void UpdateTable()
        {
            var temp = Data;
            Data = null;
            Data = temp;
        }

        private void LoadFile_Click(object sender, RoutedEventArgs e)
        {
            var fileDialog = new OpenFileDialog();
            fileDialog.Filter = "Text file (*.txt)|*.txt|CSV file(*.csv)|*.csv|All files (*.*)|*.*";
            fileDialog.FilterIndex = 4;
            var result = fileDialog.ShowDialog();
            if (result.HasValue && result.Value)
            {
                fileData = InputOutput.ProcessFile(fileDialog.FileName);
                Data = fileData.Table;
                InitializeAxisComboBoxes();
                InitializeStringClassComboBox();
                InitializeAllColumnsComboBoxes();
            }
        }

        private void SaveFile_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog fileDialog = new SaveFileDialog();
            fileDialog.Filter = "Text file (*.txt)|*.txt|CSV file(*.csv)|*.csv|All files (*.*)|*.*";
            fileDialog.FilterIndex = 2;
            fileDialog.DefaultExt = ".csv";
            fileDialog.FileName = "output.csv";
            var result = fileDialog.ShowDialog();
            if (result.HasValue && result.Value)
            {
                InputOutput.SaveFile(fileDialog.FileName, Data);
                MessageBox.Show("File " + fileDialog.FileName + " saved.");
            }
            else
            {
                MessageBox.Show("Error saving file");
            }
        }

        private void LoadExcelFile_Click(object sender, RoutedEventArgs e)
        {
            var fileDialog = new OpenFileDialog();
            fileDialog.Filter = "Excel Files|*.xls;*.xlsx;*.xlsm";
            var result = fileDialog.ShowDialog();
            if (result.HasValue && result.Value)
            {
                fileData = InputOutput.ProcessExcelFile(fileDialog.FileName);
                Data = fileData.Table;
                InitializeAxisComboBoxes();
                InitializeStringClassComboBox();
            }
        }

        private void RemoveColumn_Click(object sender, RoutedEventArgs e)
        {
            int id = Data.Columns.IndexOf(SelectedRemovalColumn);
            Data.Columns.Remove(SelectedRemovalColumn);
            UpdateTable();
            fileData.NumericalColumns.Remove(id);
            fileData.Classes.Remove(id);
            AxisValues.Remove(SelectedRemovalColumn);
            for (int i = 0; i < fileData.NumericalColumns.Count; i++)
            {
                if (fileData.NumericalColumns[i] > id) fileData.NumericalColumns[i]--;
            }
            Dictionary<int, List<string>> newClasses = new Dictionary<int, List<string>>();
            foreach (var entry in fileData.Classes)
            {
                if (entry.Key > id) newClasses.Add(entry.Key - 1, entry.Value);
                else newClasses.Add(entry.Key, entry.Value);
            }
            fileData.Classes = newClasses;
            UpdateAxisComboBoxes();
            if (AxisValues.Count > 0) SelectedX = SelectedY = SelectedDiscr = SelectedClassificationColumn = SelectedSimilarityColumn1 = SelectedSimilarityColumn2 = SelectedRemovalColumn = AxisValues[0];
            InitializeStringClassComboBox();
            InitializeAllColumnsComboBoxes();
        }
    }
}
