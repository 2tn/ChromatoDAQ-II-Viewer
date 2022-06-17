using System.Windows;
using System.IO.Ports;

namespace ChromatoDAQ_II_Viewer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 

    public partial class MainWindow : Window
    {
        public static SerialPort port;
        public MainWindow()
        {
            InitializeComponent();
            var vm = DataContext as MainWindowViewModel;
            vm.chart = new CombinationChart(vm, chartB_view);
        }
    }
}
