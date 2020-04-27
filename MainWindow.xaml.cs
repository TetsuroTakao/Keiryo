using IngicateWpf.COMPort;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace IngicateWpf
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        static MainWindowVM vm { get; set; }
        public MainWindow()
        {
            InitializeComponent();
            base.WindowStartupLocation = WindowStartupLocation.Manual;
            base.Left = 158d;
            base.Top = 0d;
            vm = DataContext as MainWindowVM;
            LoadSettingsAndCom(vm);
            vm.ViewDispatcher = this.Dispatcher;
            vm.MessageAreaText = string.Format("Port[{0}]  保存先 [{1}]", vm.ComPortInstance.ComPortName, vm.LogSaveDirctory);
            vm.CodeInputText = "";//本番
            vm.CodeInputIsEnabled = true;
            vm.EndButtonIsEnabled = true;
            vm.MeasurementStateContent = "計量データ受信処理: 計量待機";
            vm.MeasurementProgressValue = 0;
            var listData = new ObservableCollection<WeightMeasurementLog>();
            vm.MeasurementLog = listData;
            vm.AlertVisibility = false;//本番
            vm.TimeSpanSliderValue = 0;
            vm.InteraptButtonIsEnabled = false;

            CodeInput.Focus();
        }
        //void worker_DoWork(object sender, DoWorkEventArgs e)
        //{
        //    for (int i = 0; i < 100; i++)
        //    {
        //        (sender as BackgroundWorker).ReportProgress(i);
        //        Thread.Sleep(50);
        //    }
        //}
        //void worker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        //{
        //    if (e.ProgressPercentage < 100) 
        //    {
        //        vm.MeasurementProgressValue = e.ProgressPercentage;
        //    }
        //}
        private void EndButton_Click(object sender, RoutedEventArgs e)
        {
            vm.ComPortInstance.Close();
            Application.Current.Shutdown(0);
        }
        private void CodeInput_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                vm.MeasurementProgressValue = 0;
                vm.MeasurementProgressIsIndeterminate = true;
                e.Handled = true;
                var code = (e.Source as TextBox).Text;
                code = code.Replace("\r\n", "");
                vm.CodeInputText = code;
                displayUpdate(14);
                var weight = string.Empty;
                var task = new Task(new Action(() =>
                {
                    vm.ComPortInstance.Buffer = new byte[100];
                    for (int i = 1; i <= 10; i++)
                    {
                        vm.GetKeiryoData(i);
                        if (vm.ComPortInstance.Result == ComCommunicateResultType.TimeOut)
                        {
                            displayUpdate(12);
                            break;
                        }
                        else if (vm.ComPortInstance.Result == ComCommunicateResultType.Cnacel)
                        {
                            displayUpdate(13);
                            break;
                        }
                        else 
                        {
                            if (i == 10)
                            {
                                weight = UnicodeEncoding.ASCII.GetString(vm.ComPortInstance.WeightReadBuffer);
                                if (weight.Length > 4) weight = weight.Substring(0, 5);
                                var d = 0d;
                                if (double.TryParse(weight, out d))
                                {
                                    //'リストビューの追加
                                    var item = new WeightMeasurementLog();
                                    item.Code = vm.CodeInputText;
                                    item.Memo = string.Join(",", vm.ComPortInstance.Messages);
                                    item.Memo += weight;
                                    item.Weight = d;
                                    item.WeightString = vm.SaveLog(vm.CodeInputText, weight).ToString() + " kg";
                                    item.EventOccours = DateTime.Now;
                                    item.EventMessage = item.EventOccours.ToString("yy/MM/dd HH:mm:ss");
                                    var list = vm.MeasurementLog;
                                    list.Add(item);
                                    vm.MeasurementLog = list;
                                }
                            }
                            if (i == 3)
                            {
                                var text = string.Empty;
                                if (vm.ComPortInstance.WeightReadBuffer == null) vm.ComPortInstance.WeightReadBuffer = new byte[0];
                                vm.AlertText = string.Format("（BufferLength:{0}）（WeightReadBufferLength:{1}）（BufferToString:{2}）（WeightReadBuffer:{3}）", (vm.ComPortInstance.Buffer == null ? 99 : vm.ComPortInstance.Buffer.Length), vm.ComPortInstance.WeightReadBuffer.Length, vm.ComPortInstance.Messages.LastOrDefault(), vm.debugText);
                                vm.AlertVisibility = true;
                            }
                            displayUpdate(i);
                        }
                        Thread.Sleep(100);
                    }
                    displayUpdate(15);
                }));
                task.Start();
                task.ContinueWith(new Action<Task>((t) => 
                {
                    if (vm.ComPortInstance.Result== ComCommunicateResultType.Cnacel)
                    {
                        displayUpdate(13);
                    }
                }));
            }
        }
        void displayUpdate(int i)
        {
            switch (i)
            {
                case 1:
                    //'１．計量データ取得：ENQ受信
                    Dispatcher.BeginInvoke(new Action(() =>
                    {
                        vm.MeasurementStateContent = "１．計量データ取得：ENQ受信中";
                        vm.MeasurementProgressIsIndeterminate = false;
                        vm.MeasurementProgressValue = 10;
                    }));
                    break;
                case 2:
                    //'２．計量データ取得：ACK送信
                    Dispatcher.BeginInvoke(new Action(() =>
                    {
                        vm.MeasurementStateContent = "２．計量データ取得：ACK送信中";
                        vm.MeasurementProgressValue += 10;
                    }));
                    break;
                case 3:
                    //'３．計量データ取得：STX + テキスト + ETX受信
                    Dispatcher.BeginInvoke(new Action(() =>
                    {
                        vm.MeasurementStateContent = "３．計量データ取得：STX + テキスト + ETX受信";
                        vm.MeasurementProgressValue += 10;
                    }));
                    break;
                case 4:
                    //'４．計量データ取得：ACK送信
                    Dispatcher.BeginInvoke(new Action(() =>
                    {
                        vm.MeasurementStateContent = "４．計量データ取得：ACK送信中";
                        vm.MeasurementProgressValue += 10;
                    }));
                    break;
                case 5:
                    //'５．計量データ取得：EOT受信
                    Dispatcher.BeginInvoke(new Action(() =>
                    {
                        vm.MeasurementStateContent = "５．計量データ取得：EOT受信中";
                        vm.MeasurementProgressValue += 10;
                    }));
                    break;
                case 6:
                    //'６．正常終了通知：ENQ送信
                    Dispatcher.BeginInvoke(new Action(() =>
                    {
                        vm.MeasurementStateContent = "６．正常終了通知：ENQ送信中";
                        vm.MeasurementProgressValue += 10;
                    }));
                    break;
                case 7:
                    //'７．正常終了通知：ACK受信
                    Dispatcher.BeginInvoke(new Action(() =>
                    {
                        vm.MeasurementStateContent = "７．正常終了通知：ACK受信中";
                        vm.MeasurementProgressValue += 10;
                    }));
                    break;
                case 8:
                    //'８．正常終了通知：STX + '9271' + ETX送信中
                    Dispatcher.BeginInvoke(new Action(() =>
                    {
                        vm.MeasurementStateContent = "８．正常終了通知：STX + '9271' + ETX送信中";
                        vm.MeasurementProgressValue += 10;
                    }));
                    break;
                case 9:
                    //'９.正常終了通知() : ACK受信()
                    Dispatcher.BeginInvoke(new Action(() =>
                    {
                        vm.MeasurementStateContent = "９．正常終了通知：ACK受信中";
                        vm.MeasurementProgressValue += 10;
                    }));
                    break;
                case 10:
                    //'１０．正常終了通知：EOT送信
                    Dispatcher.BeginInvoke(new Action(() =>
                    {
                        vm.MeasurementStateContent = "１０．正常終了通知：EOT送信中";
                        vm.MeasurementProgressValue += 10;
                    }));
                    break;
                case 11:
                    //'１０．正常終了通知：EOT送信
                    Dispatcher.BeginInvoke(new Action(() =>
                    {
                        vm.MeasurementStateContent = "処理を中断しています･･･";
                    }));
                    break;
                case 12:
                    Dispatcher.BeginInvoke(new Action(() =>
                    {
                        vm.MeasurementStateContent = "計量データ受信処理: 計量待機";
                        vm.AlertText = "計量処理でタムアウトが発生しました。計量は完了していません";
                        vm.AlertVisibility = true;
                        vm.MeasurementProgressValue = 0;
                    }));
                    break;
                case 13:
                    Dispatcher.BeginInvoke(new Action(() =>
                    {
                        vm.MeasurementStateContent = "計量データ受信処理: 計量待機";
                        vm.AlertText = "計量処理でキャンセルが発生しました。計量は完了していません";
                        vm.AlertVisibility = true;
                        vm.MeasurementProgressValue = 0;
                    }));
                    break;
                case 14:
                    Dispatcher.BeginInvoke(new Action(() =>
                    {
                        vm.CodeInputIsEnabled = false;
                        vm.EndButtonIsEnabled = false;
                        vm.InteraptButtonIsEnabled = true;
                    }));
                    break;
                case 15:
                    Dispatcher.BeginInvoke(new Action(() =>
                    {
                        vm.MeasurementStateContent = "計量データ受信処理: 計量待機";
                        vm.InteraptButtonIsEnabled = false;
                        vm.CodeInputIsEnabled = true;
                        vm.EndButtonIsEnabled = true;
                        vm.ComPortInstance.Result = ComCommunicateResultType.None;
                        CodeInput.Focus();
                        vm.CodeInputText = string.Empty;
                    }));
                    break;
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                vm.ComPortInstance.Result = ComCommunicateResultType.Cnacel;
            }));
            displayUpdate(11);
        }
        void LoadSettingsAndCom(MainWindowVM vm) 
        {
            try 
            {
                //App.configと差し替え
                //var inifile = "YAMA_KEIRYO.ini";
                //保存先フォルダとCOMポート、ファイル名の初期値
                vm.LogSaveDirctory = Environment.CurrentDirectory + @"\Keiryo";
                var com = "COM1";
                //App.configで上書き
                if (!string.IsNullOrEmpty(ConfigurationManager.AppSettings["SAVE_DIR2"])) vm.LogSaveDirctory = ConfigurationManager.AppSettings["SAVE_DIR2"];
                if (!string.IsNullOrEmpty(ConfigurationManager.AppSettings["COM_PORT"])) com = ConfigurationManager.AppSettings["COM_PORT"];
                if (!string.IsNullOrEmpty(ConfigurationManager.AppSettings["SAVE_FILE"])) vm.LogSaveFileName = ConfigurationManager.AppSettings["SAVE_FILE"];
                vm.ComPortInstance = new COMSerialPort(com);
                //vm.ComPortInstance.Open();
                if (vm.ComPortInstance.Messages.Count > 0) 
                {
                    vm.AlertText = string.Join(",", vm.ComPortInstance.Messages);
                    vm.AlertVisibility = true;
                    Thread.Sleep(5000);
                    vm.AlertVisibility = false;
                }
            }
            catch (Exception ex)
            {
                vm.AlertText = ex.Message;
                vm.AlertVisibility = true;
                Thread.Sleep(5000);
                vm.AlertVisibility = false;
            }
        }
    }
}
