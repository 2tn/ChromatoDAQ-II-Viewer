using System;
using System.Windows.Forms;
using System.Windows.Input;

namespace ChromatoDAQ_II_Viewer
{
    public class ButtonCommands
    {
        public class SelectFolderCommand : ICommand
        {
            public event EventHandler CanExecuteChanged;
            private MainWindowViewModel vm;
            public SelectFolderCommand(MainWindowViewModel viewModel)
            {
                vm = viewModel;
            }

            public bool CanExecute(object parameter) { return true; }

            public void Execute(object parameter)
            {
                using (var dialog = new FolderBrowserDialog())
                {
                    DialogResult result = dialog.ShowDialog();
                    if (result == DialogResult.OK)
                        vm.SavePath = dialog.SelectedPath;
                }
            }
        }

        public class YRangeCommand : ICommand
        {
            public event EventHandler CanExecuteChanged;
            private MainWindowViewModel vm;
            public YRangeCommand(MainWindowViewModel viewModel)
            {
                vm = viewModel;
            }

            public bool CanExecute(object parameter) { return true; }

            public void Execute(object parameter)
            {
                var curr = vm.chart.getYRange();
                if (Math.Abs(curr[1]) == vm.chart.YRangeSmall)
                {
                    vm.chart.setYRange(-vm.chart.YRangeLarge, vm.chart.YRangeLarge, vm.chart.YRangeLargeInterval);
                    vm.YRangeText = "Y軸範囲縮小";
                }
                else
                {
                    vm.chart.setYRange(-vm.chart.YRangeSmall, vm.chart.YRangeSmall, vm.chart.YRangeSmallInterval);
                    vm.YRangeText = "Y軸範囲拡大";
                }
            }
        }
    }
}
