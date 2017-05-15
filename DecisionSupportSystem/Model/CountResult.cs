using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DecisionSupportSystem.Model
{
    public class CountResult
    {
        public int Count { get; set; }
        public bool IsAscending { get; set; }
        public int Column { get; set; }
        public double MaxValue { get; set; }
        public string Class { get; set; }
    }
}
