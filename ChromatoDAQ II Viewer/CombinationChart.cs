using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms.DataVisualization.Charting;

namespace ChromatoDAQ_II_Viewer
{
    public class CombinationChart
    {
        private MainWindowViewModel vm;
        private Chart chart;
        public Series series1, series2;
        Font labelFont = new Font("Arial", 10);
        Font titleFont = new Font("Arial", 12);
        public CombinationChart(MainWindowViewModel viewModel, Chart chart)
        {
            vm = viewModel;
            this.chart = chart;
            chart.ChartAreas.Add("ChartArea1");
            chart.ChartAreas[0].AxisX.MajorGrid.LineColor = Color.DarkGray;
            chart.ChartAreas[0].AxisX.Title = "Time [s]";
            chart.ChartAreas[0].AxisX.TitleFont = titleFont;
            chart.ChartAreas[0].AxisX.LabelStyle.Font = labelFont;
            chart.ChartAreas[0].AxisX.Minimum = 0;
            chart.ChartAreas[0].AxisX.Maximum = 30;
            chart.ChartAreas[0].AxisY.Title = "Voltage [mV]";
            chart.ChartAreas[0].AxisY.TitleFont = titleFont;
            chart.ChartAreas[0].AxisY.LabelStyle.Font = labelFont;
            chart.ChartAreas[0].AxisY.Interval = 0.5;
            chart.ChartAreas[0].AxisY.Minimum = -2;
            chart.ChartAreas[0].AxisY.Maximum = 2;


            series1 = new Series();
            series1.ChartType = SeriesChartType.Line;
            series1.MarkerStyle = MarkerStyle.None;
            series1.MarkerColor = Color.Red;
            series1.Color = Color.DeepPink;

            series2 = new Series();
            series2.ChartType = SeriesChartType.Line;
            series2.MarkerStyle = MarkerStyle.None;
            series2.MarkerColor = Color.Blue;
            series2.Color = Color.DeepSkyBlue;

            chart.Series.Add(series1);
            chart.Series.Add(series2);

        }
        int Pow10(int x)
        {
            int ret = 1;
            for (int i = 0; i < x; i++)
                ret *= 10;
            return ret;
        }

        public void Add(Series series, double x, double y)
        {
            series.Points.AddXY(x, y);
            double xmin = double.MaxValue, xmax = double.MinValue;
            foreach (var p in series.Points)
            {
                xmin = Math.Min(xmin, p.XValue);
                xmax = Math.Max(xmax, p.XValue);
            }
        }

        public void Add(List<Data> list)
        {
            foreach (var data in list)
            {
                Add(series1, data.Time, data.Ch1);
                Add(series2, data.Time, data.Ch2);
            }
        }
        public void Clear()
        {
            foreach (var series in chart.Series)
            {
                series.Points.Clear();
            }
            chart.ChartAreas[0].AxisX.Maximum = 30;
        }

        public void setTimeMax(double time)
        {
            if (time > chart.ChartAreas[0].AxisX.Minimum)
                chart.ChartAreas[0].AxisX.Maximum = time;
        }

        private bool chart1Visible = true;
        public bool Chart1Visible
        {
            get { return chart1Visible; }
            set
            {
                chart1Visible = value;
                series1.Enabled = value;
            }
        }

        private bool chart2Visible = true;
        public bool Chart2Visible
        {
            get { return chart2Visible; }
            set
            {
                chart2Visible = value;
                series2.Enabled = value;
            }
        }
    }
}
