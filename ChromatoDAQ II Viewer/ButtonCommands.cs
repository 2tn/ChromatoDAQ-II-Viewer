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
    }
}
