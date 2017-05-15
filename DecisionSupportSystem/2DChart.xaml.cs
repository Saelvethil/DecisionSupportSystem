using Microsoft.Research.DynamicDataDisplay.Common;
using Microsoft.Research.DynamicDataDisplay.DataSources;
using Microsoft.Research.DynamicDataDisplay;
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
using System.Windows.Shapes;
using DecisionSupportSystem.Model;
using Microsoft.Research.DynamicDataDisplay.Charts;
using Microsoft.Research.DynamicDataDisplay.PointMarkers;

namespace DecisionSupportSystem
{
    /// <summary>
    /// Window with 2D Chart
    /// </summary>
    public partial class _2DChart : Window
    {
        /// <summary>
        /// List of Data Points
        /// Grouped in RingArrays By Respective Classes
        /// </summary>
        public List<RingArray<Point>> Points;
        public int MinValueX { get; set; }
        public int MaxValueX { get; set; }
        public int MinValueY { get; set; }
        public int MaxValueY { get; set; }
        public string AxisNameY { get; set; }
        public string AxisNameX { get; set; }

        public int CutsStart { get; set; }

        /// <summary>
        /// For the Legend to Show Class Colors in Order
        /// </summary>
        public List<string> ClassNames { get; set; }

        List<SolidColorBrush> brushes;

        public _2DChart(GraphModel graphModel, bool isCuts)
        {
            InitializeComponent();
            this.DataContext = this;

            Points = graphModel.Points;
            MinValueX = graphModel.MinValueX;
            MaxValueX = graphModel.MaxValueX;
            AxisNameX = graphModel.AxisNameX;
            MinValueY = graphModel.MinValueY;
            MaxValueY = graphModel.MaxValueY;
            AxisNameY = graphModel.AxisNameY;
            ClassNames = graphModel.ClassNames;
            CutsStart = graphModel.CutsStart;

            brushes = new List<SolidColorBrush> { Brushes.Black, Brushes.Red, Brushes.Orange, Brushes.Yellow,  Brushes.Green, Brushes.Cyan, Brushes.Blue, Brushes.Indigo, Brushes.Violet, Brushes.Brown,
                                                            Brushes.Turquoise, Brushes.Firebrick, Brushes.DarkRed, Brushes.DarkOliveGreen, Brushes.Fuchsia, Brushes.YellowGreen, Brushes.PeachPuff, Brushes.IndianRed, Brushes.Moccasin,
                                                            Brushes.Navy, Brushes.Orchid, Brushes.Chocolate, Brushes.Maroon, Brushes.DodgerBlue};
            for (int i = 1; i < 255; i++)
            {
                brushes.Add(new SolidColorBrush(Color.FromArgb(255, (byte)i, 0, 0)));
            }
            for (int i = 1; i < 255; i++)
            {
                brushes.Add(new SolidColorBrush(Color.FromArgb(255, 0, (byte)i, 0)));
            }

            for (int i = 0; i < Points.Count; i++)
            {
                var dataSrc = new EnumerableDataSource<Point>(Points[i]);
                dataSrc.SetXMapping(pt => pt.X);
                dataSrc.SetYMapping(pt => pt.Y);

                Pen pen = new Pen(brushes[i], 3);
                Pen penTransparent = new Pen(Brushes.Transparent, 3);
                PenDescription description;
                if (ClassNames != null) description = new PenDescription(ClassNames[i]);
                else description = new PenDescription("Classless");

                // size here
                int pointSize = 5;

                if (isCuts && i >= CutsStart)
                {
                    plotter.AddLineGraph(dataSrc, pen, new CirclePointMarker() { Fill = brushes[i], Size = pointSize }, description);
                }
                else
                {
                    if (Points[i].Count > 1)
                    {
                        EnumerableDataSource<Point> point = new EnumerableDataSource<Point>(new RingArray<Point>(1) { Points[i][0] });
                        point.SetXMapping(pt => pt.X);
                        point.SetYMapping(pt => pt.Y);
                        plotter.AddLineGraph(point, pen, new CirclePointMarker() { Fill = brushes[i], Size = pointSize }, description);
                    }
                    else if (Points[i].Count == 1)
                    {
                        plotter.AddLineGraph(dataSrc, pen, new CirclePointMarker() { Fill = brushes[i], Size = pointSize }, description);
                    }
                    plotter.AddLineGraph(dataSrc, penTransparent, new CirclePointMarker() { Fill = brushes[i], Size = pointSize }, new PenDescription(" "));
                }
            }
        }
    }
}
