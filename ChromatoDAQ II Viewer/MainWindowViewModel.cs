using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO.Ports;
using static ChromatoDAQ_II_Viewer.ChromatoDAQ;
using static ChromatoDAQ_II_Viewer.ButtonCommands;
using System.Reflection;
using System.Collections.Generic;

namespace ChromatoDAQ_II_Viewer
{
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        public ChromatoDAQ chromatoDAQ;
        public CombinationChart chart;

        public ObservableCollection<String> PortItems { get; set; }
        public MainWindowViewModel()
        {
            PortItems = new();
            foreach (var p in SerialPort.GetPortNames())
                PortItems.Add(p);
            chromatoDAQ = new();
            StartCommand = new StartCommand(this);
            SelectFolderCommand = new SelectFolderCommand(this);
        }

        public string WindowTitle { get { return $"ChromatoDAQ_II_Viewer Version: {Assembly.GetExecutingAssembly().GetName().Version.ToString(2)}"; } }

        public List<Data> DataList = new();

        private string savePath;
        public string SavePath
        {
            get { return savePath; }
            set
            {
                savePath = value;
                OnPropertyChanged(nameof(SavePath));
            }
        }

        private string iPAddress = "192.168.0.100";
        public string IPAddress { get { return iPAddress; } set { iPAddress = value; } }

        private string seriesName = DateTime.Now.ToString("yyyy_MM_dd");
        public string SeriesName { get { return seriesName; } set { seriesName = value; OnPropertyChanged(nameof(SeriesName)); } }

        private string startStatusText = "開始";
        public string StartStatusText
        { get { return startStatusText; } set { startStatusText = value; OnPropertyChanged(nameof(StartStatusText)); } }

        private bool channel1Checked = true;
        public bool Channel1Checked
        {
            get { return channel1Checked; }
            set
            {
                channel1Checked = value;
                chart.Chart1Visible = value;
            }
        }

        private bool channel2Checked = true;
        public bool Channel2Checked
        {
            get { return channel2Checked; }
            set
            {
                channel2Checked = value;
                chart.Chart2Visible = value;
            }
        }

        public SelectFolderCommand SelectFolderCommand
        { get; private set; }
        public StartCommand StartCommand
        { get; private set; }

        private bool startBackground = false;
        public bool StartBackground
        { get { return startBackground; } set { startBackground = value; OnPropertyChanged(nameof(StartBackground)); } }

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string info)
        { PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(info)); }

        private int ChartCounter = 0;

        public void DrawGraph()
        {
            int length = DataList.Count;
            chart.Add(DataList.GetRange(ChartCounter, length - ChartCounter));
            ChartCounter = length;
            var timeMax = DataList[DataList.Count - 1].Time;
            if (timeMax > 30)
                chart.setTimeMax((Math.Ceiling(timeMax / 10)) * 10);
        }

        public void ClearChart()
        {
            DataList.Clear();
            chart.Clear();
            ChartCounter = 0;
        }
    }
}
