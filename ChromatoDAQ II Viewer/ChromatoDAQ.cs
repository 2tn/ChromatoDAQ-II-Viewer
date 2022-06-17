using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.IO.Ports;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Input;

namespace ChromatoDAQ_II_Viewer
{
    public class ChromatoDAQ
    {
        private TcpClient client;
        public Task Task { get; set; }
        public bool IsWorking { get; set; } = false;
        public ChromatoDAQ()
        {
            client = new TcpClient();
        }

        public bool Connected { get { return client.Connected; } }

        public void Open(string iPAddress)
        {
            try
            {
                if (client.Connected == false)
                {
                    client = new TcpClient();
                    client.Connect(iPAddress, 60000);
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, "エラー");
            }
        }

        public void Close()
        {
            try
            {
                if (client.Connected == true)
                {
                    client.Close();
                    client.Dispose();
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, "エラー");
            }
        }

        public bool getIdentification()
        {
            if (client.Connected)
            {
                Write("@V\r");
                Task.Delay(100);
                var result = ReceiveDataAll();
                return result.Contains("TYPE C-2");
            }
            return false;
        }

        public void sendStart(int interval)
        {
            interval = (int)(Math.Round((double)interval / 10));
            if (client.Connected)
            {
                client.GetStream().Flush();
                Write($"@C,{interval},0,0,1\r");
            }
        }

        public void sendAbort()
        {
            if (client.Connected)
                Write("@B\r");
        }

        public string ReceiveDataAll()
        {
            var stream = client.GetStream();
            byte[] buffer = new byte[10000];
            var data = stream.Read(buffer, 0, 10000);
            return Encoding.ASCII.GetString(buffer);
        }

        public string ReceiveDataLine()
        {
            byte[] buffer = new byte[1];
            string ret = string.Empty;

            while (true)
            {
                try { client.GetStream().Read(buffer, 0, 1); }
                catch (TimeoutException) { break; }
                ret += (char)buffer[0];

                if (ret.EndsWith('\r'))
                {
                    // Truncate the line ending
                    Debug.WriteLine($"ReceiveDataLine: {ret.Substring(0, ret.Length - 1)}");
                    return ret.Substring(0, ret.Length - 1);
                }
            }

            return string.Empty;
        }
        public string ReceiveData()
        {
            byte[] receiveBuffer = new byte[100000];
            int bytesReceived = client.GetStream().Read(receiveBuffer);
            return Encoding.ASCII.GetString(receiveBuffer.AsSpan(0, bytesReceived));
        }

        public void Write(string str)
        {
            var buffer = Encoding.UTF8.GetBytes(str);
            client.GetStream().Write(buffer, 0, buffer.Length);
        }

        public int getDataAvailable()
        {
            return client.Available;
        }

        public class StartCommand : ICommand
        {
            public event EventHandler CanExecuteChanged;
            private MainWindowViewModel vm;
            private CancellationTokenSource tokenSource;
            public StartCommand(MainWindowViewModel viewModel)
            {
                vm = viewModel;
            }

            public bool CanExecute(object parameter) { return true; }

            public async void Execute(object parameter)
            {
                ChromatoDAQ chromatoDAQ = vm.chromatoDAQ;

                if (!chromatoDAQ.IsWorking)
                {
                    chromatoDAQ.Open(vm.IPAddress);
                    chromatoDAQ.sendAbort();
                    if (chromatoDAQ.Connected)
                    {
                        if (string.IsNullOrEmpty(vm.SavePath))
                        {
                            MessageBox.Show("フォルダを選択してください", "エラー");
                            return;
                        }
                        if (File.Exists($"{vm.SavePath}\\{vm.SeriesName}.csv"))
                        {
                            MessageBox.Show("すでに指定された名前のファイルが存在します", "エラー");
                            return;
                        }

                        // データ取得
                        vm.ClearChart();
                        vm.StartBackground = true;
                        vm.StartStatusText = "停止";
                        tokenSource = new();
                        chromatoDAQ.Task = Measure(vm, tokenSource.Token);
                        chromatoDAQ.IsWorking = true;
                    }
                    else
                    {
                        chromatoDAQ.Close();
                        MessageBox.Show("接続できませんでした。", "エラー");
                    }

                }
                else
                {
                    if (chromatoDAQ.client.Connected)
                    {
                        chromatoDAQ.sendAbort();
                        tokenSource.Cancel();
                        chromatoDAQ.IsWorking = false;
                        vm.StartBackground = false;
                        vm.StartStatusText = "開始";
                        await Task.Delay(100);
                        chromatoDAQ.Close();
                    }
                    else
                        MessageBox.Show("接続されていません。", "エラー");
                }
            }
        }

        private static async Task Measure(MainWindowViewModel vm, CancellationToken ct)
        {
            ChromatoDAQ chromatoDAQ = vm.chromatoDAQ;

            // 測定コマンドを送信
            chromatoDAQ.sendStart(10);

            // 測定開始できた場合
            // CSVヘッダー書き換え
            string path = $"{vm.SavePath}\\{vm.SeriesName}.csv";
            string filename = $"{vm.SeriesName}.csv";
            string csvStr = "Time,Ch1,Ch2,Time\r\n";
            File.AppendAllText(path, csvStr);

            int time = 0;
            string lastline = string.Empty;
            while (true)
            {
                if (ct.IsCancellationRequested) 
                    break;
                if (chromatoDAQ.getDataAvailable() != 0)
                {
                    string res = chromatoDAQ.ReceiveData();
                    string newlastline = res.Substring(res.LastIndexOf('\r') + 1, res.Length - res.LastIndexOf('\r') - 1);
                    string str = lastline + res.Substring(0, res.LastIndexOf('\r'));
                    lastline = newlastline;
                    var lines = str.Split('\r');

                    List<Data> list = new();
                    double coeff = 0.00014901162;

                    foreach (var line in lines)
                    {
                        // Debug.WriteLine(line);
                        if (line == "C" || line == "B") continue;
                        var result = Regex.Match(line, " *(\\d*), *(\\d*), *(\\d*)");
                        if (result.Groups.Count == 4)
                        {
                            var data = new Data((double)time / 1000, double.Parse(result.Groups[1].Value) * coeff, double.Parse(result.Groups[2].Value) * coeff, int.Parse(result.Groups[3].Value));
                            list.Add(data);
                            time += 10;
                        }
                    }
                    _ = SaveFile(vm, list, path, filename);
                }
                await Task.Delay(100);
            }
        }

        private static async Task SaveFile(MainWindowViewModel vm, List<Data> data, string path, string filename)
        {
            vm.DataList.AddRange(data);
            vm.DrawGraph();

            string csvStr = string.Empty;
            foreach (var d in data)
                csvStr += $"{d.Time},{d.Ch1},{d.Ch2},{d.Input}\r\n";

            await File.AppendAllTextAsync(path, csvStr);
        }
    }
}
