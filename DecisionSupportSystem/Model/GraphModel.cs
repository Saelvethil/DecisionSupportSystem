using Microsoft.Research.DynamicDataDisplay.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace DecisionSupportSystem.Model
{
    /// <summary>
    /// Data Model for 2D Graph
    /// </summary>
    public class GraphModel
    {
        public List<RingArray<Point>> Points;
        public int MinValueX { get; set; }
        public int MaxValueX { get; set; }
        public int MinValueY { get; set; }
        public int MaxValueY { get; set; }
        public string AxisNameY { get; set; }
        public string AxisNameX { get; set; }
        public List<string> ClassNames { get; set; }
        public int CutsStart { get; set; }
    }
}
