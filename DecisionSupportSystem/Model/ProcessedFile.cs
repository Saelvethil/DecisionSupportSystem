using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DecisionSupportSystem.Model
{
    /// <summary>
    /// Data Model for Processed File
    /// </summary>
    public class ProcessedFile
    {
        public DataTable Table { get; set; }
        public Dictionary<int, List<string>> Classes { get; set; }
        public List<int> NumericalColumns { get; set; }
    }
}
