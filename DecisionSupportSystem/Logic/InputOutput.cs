using DecisionSupportSystem.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DecisionSupportSystem.Logic
{
    public static class InputOutput
    {
        public static bool IsNumber(string str)
        {
            float tmp;
            return float.TryParse(str, NumberStyles.Any, CultureInfo.InvariantCulture, out tmp);
        }

        public static ProcessedFile ProcessFile(string path)
        {
            ProcessedFile output = new ProcessedFile();

            DataTable table = new DataTable();

            StreamReader reader = new StreamReader(path);
            string line;

            // skip comment lines
            while ((line = reader.ReadLine()) != null)
            {
                if (line.StartsWith("#") || String.IsNullOrWhiteSpace(line)) continue;
                else break;
            }
            string[] values = line.Split(' ', '\t', ';', ',');
            bool isNumber = false;

            // check if first row has no numbers, if so then it's header row
            foreach (string str in values)
            {
                if (IsNumber(str)) isNumber = true;
                break;
            }

            // filler headers or column names if header row present
            if (isNumber)
            {
                reader.DiscardBufferedData();
                reader.BaseStream.Seek(0, SeekOrigin.Begin);
                for (int i = 1; i <= values.Count(); i++)
                {
                    table.Columns.Add("Column" + i);
                }
            }
            else
            {
                foreach (string str in values)
                {
                    table.Columns.Add(str);
                }
            }

            Dictionary<int, List<string>> classes = new Dictionary<int, List<string>>();
            line = reader.ReadLine();
            values = line.Split(' ', '\t', ';', ',');

            List<int> numericalColumns = new List<int>();

            //initialize dictionary rows and get indexes of numerical columns
            for (int i = 0; i < values.Length; i++)
            {
                if (!IsNumber(values[i])) classes.Add(i, new List<string>());
                else numericalColumns.Add(i);
            }

            // fill the table, change strings for class numbers
            do
            {
                if (line.StartsWith("#") || String.IsNullOrWhiteSpace(line)) continue;
                else
                {
                    values = line.Split(' ', '\t', ';', ',');
                    DataRow row = table.NewRow();
                    for (int i = 0; i < values.Length; i++)
                    {
                        row.SetField(i, values[i]);
                    }
                    table.Rows.Add(row);
                }
            }
            while ((line = reader.ReadLine()) != null);
            reader.Close();

            output.Table = table;
            output.Classes = classes;
            output.NumericalColumns = numericalColumns;
            return output;
        }

        public static ProcessedFile ProcessExcelFile(string path)
        {
            ProcessedFile output = new ProcessedFile();
            DataTable table = new DataTable();



            return output;
        }

        public static void SaveFile(string path, DataTable data)
        {
            string ext = Path.GetExtension(path);
            char sep = ' ';
            if (ext == ".csv") sep = ',';
            StreamWriter writer = new StreamWriter(path, false);
            {
                for (int i = 0; i < data.Columns.Count - 1; i++)
                {
                    writer.Write(data.Columns[i].ToString() + sep);
                }
                writer.WriteLine(data.Columns[data.Columns.Count - 1].ToString());
            }

            for (int i = 0; i < data.Rows.Count; i++)
            {
                for (int j = 0; j < data.Columns.Count - 1; j++)
                {
                    writer.Write(data.Rows[i][j].ToString() + sep);
                }
                writer.WriteLine(data.Rows[i][data.Columns.Count - 1].ToString());
            }
            writer.Close();
        }

        public static void SaveFileVectors(string path, DataTable data, List<List<int>> vectors, List<bool> isPointIgnored)
        {
            string ext = Path.GetExtension(path);
            char sep = ' ';
            if (ext == ".csv") sep = ',';
            StreamWriter writer = new StreamWriter(path, false);

            for (int j = 0; j < vectors[0].Count; j++) writer.Write("Vector" + j + sep);
            writer.WriteLine("Class");

            for (int i = 0; i < vectors.Count; i++)
            {
                if(!isPointIgnored[i])
                {
                    for (int j = 0; j < vectors[i].Count; j++)
                    {
                        writer.Write(vectors[i][j].ToString() + sep);
                    }
                    writer.WriteLine(data.Rows[i][data.Columns.Count - 1].ToString());
                }
            }
            writer.Close();
        }

        public static void SaveFileVectorsIgnored(string path, DataTable data, List<List<int>> vectors, List<bool> isPointIgnored)
        {
            string ext = Path.GetExtension(path);
            char sep = ' ';
            if (ext == ".csv") sep = ',';
            StreamWriter writer = new StreamWriter(path, false);

            for (int j = 0; j < vectors[0].Count; j++) writer.Write("Vector" + j + sep);
            writer.WriteLine("Class");

            for (int i = 0; i < vectors.Count; i++)
            {
                if (isPointIgnored[i])
                {
                    for (int j = 0; j < vectors[i].Count; j++)
                    {
                        writer.Write(vectors[i][j].ToString() + sep);
                    }
                    writer.WriteLine(data.Rows[i][data.Columns.Count - 1].ToString());
                }
            }
            writer.Close();
        }
    }
}
