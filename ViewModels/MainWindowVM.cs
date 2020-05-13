using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using System.Linq;
using System.Windows;
using IngicateWpf.COMPort;
using System.IO;
using System.Threading;
using System.Windows.Threading;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;
using System.Diagnostics;
using System.Security;

namespace IngicateWpf
{
    class MainWindowVM : INotifyPropertyChanged
    {
        public string debugText { get; set; }
        bool _interaptButtonIsEnabled { get; set; }
        public bool InteraptButtonIsEnabled
        {
            get
            {
                return _interaptButtonIsEnabled;
            }
            set
            {
                _interaptButtonIsEnabled = value;
                RaisePropertyChanged();
            }
        }
        string strText = string.Empty;
        public bool _measurementProgressIsIndeterminate { get; set; }
        public bool MeasurementProgressIsIndeterminate
        {
            get
            {
                return _measurementProgressIsIndeterminate;
            }
            set
            {
                _measurementProgressIsIndeterminate = value;
                RaisePropertyChanged();
            }
        }
        public Dispatcher ViewDispatcher { get; set; }
        public COMSerialPort ComPortInstance { get; set; }
        string _alertText { get; set; }
        public string AlertText
        {
            get
            {
                return _alertText;
            }
            set
            {
                _alertText = value;
                RaisePropertyChanged();
            }
        }
        int _timeSpanSliderValue { get; set; }
        public int TimeSpanSliderValue
        {
            get
            {
                return _timeSpanSliderValue;
            }
            set
            {
                _timeSpanSliderValue = value;
                RaisePropertyChanged();
            }
        }
        bool _alertVisibility { get; set; }
        public bool AlertVisibility
        {
            get
            {
                return _alertVisibility;
            }
            set
            {
                _alertVisibility = value;
                RaisePropertyChanged();
            }
        }
        string _codeInputText { get; set; }
        public string CodeInputText
        {
            get
            {
                return _codeInputText;
            }
            set
            {
                _codeInputText = value;
                RaisePropertyChanged();
            }
        }
        public string LogPassDirctory { get; set; }
        public string LogExecution { get; set; }
        bool _codeInputIsEnabled { get; set; }
        public bool CodeInputIsEnabled
        {
            get
            {
                return _codeInputIsEnabled;
            }
            set
            {
                _codeInputIsEnabled = value;
                RaisePropertyChanged();
            }
        }
        bool _endButtonIsEnabled { get; set; }
        public bool EndButtonIsEnabled
        {
            get
            {
                return _endButtonIsEnabled;
            }
            set
            {
                _endButtonIsEnabled = value;
                RaisePropertyChanged();
            }
        }
        string _messageAreaText { get; set; }
        public string MessageAreaText
        {
            get
            {
                return _messageAreaText;
            }
            set
            {
                _messageAreaText = value;
                RaisePropertyChanged();
            }
        }
        string _measurementStateContent { get; set; }
        public string MeasurementStateContent 
        {
            get
            {
                return _measurementStateContent;
            }
            set 
            {
                _measurementStateContent = value;
                RaisePropertyChanged();
            }
        }
        int _measurementProgressValue;
        public int MeasurementProgressValue 
        {
            get 
            {
                return _measurementProgressValue;
            }
            set
            {
                _measurementProgressValue = value;
                RaisePropertyChanged();
            }
        }
        ObservableCollection<WeightMeasurementLog> _measurementLog { get; set; }
        public ObservableCollection<WeightMeasurementLog> MeasurementLog
        {
            get
            {
                var result = new ObservableCollection<WeightMeasurementLog>(_measurementLog.OrderByDescending(r => r.EventOccours));
                return result;
            }
            set
            {
                _measurementLog = value;
                RaisePropertyChanged();
            }
        }
        public event PropertyChangedEventHandler PropertyChanged;
        public void GetKeiryoData(int step) 
        {
            var item = string.Empty;
            var result = -1;
            do
            {
                var j = 0;
                if (ComPortInstance.Result == ComCommunicateResultType.Cnacel) break;
                result = CommKeiryoki(step);
                j++;
                //Thread.Sleep(500);
                if (result > 0)
                {
                    if (ComPortInstance.Result == ComCommunicateResultType.Cnacel) break;
                    ComPortInstance.Result = ComCommunicateResultType.Success;
                    break;
                }
                else if (result == -2)
                {
                    ComPortInstance.Result = ComCommunicateResultType.Cnacel;
                    break;
                }
                else if (result == -3)
                {
                    if (ComPortInstance.Result == ComCommunicateResultType.Cnacel) break;
                    ComPortInstance.Result = ComCommunicateResultType.Success;
                    break;
                }
                else if (result == -4)
                {
                    if (ComPortInstance.Result == ComCommunicateResultType.Cnacel) break;
                    ComPortInstance.Result = ComCommunicateResultType.Exception;
                    break;
                }
                if (step == 3)
                {
                    if (result == -3) break;
                    //if (j > 10) break;
                }
                else 
                {
                    //if (j > 4) break;
                }
            } while (result < 1);
        }
        //'計量機との通信処理
        int CommKeiryoki(int step)
        {
            if (ComPortInstance.Result == ComCommunicateResultType.Cnacel) return -2;
            ComPortInstance.Result = ComCommunicateResultType.TimeOut;
            int result = 0;
            byte[] sentByte = new byte[0];
            var wkBuffer = new byte[0];
            try
            {
                switch (step)
                {
                    case 1:
                        //'データ受信
                        wkBuffer = new byte[1];
                        result = ComPortInstance.GetBytes(COMSerialPort.ENQ, wkBuffer);
                        ComPortInstance.Buffer[1] = wkBuffer[0];
                        #if DEBUG
                            result = COMSerialPort.ENQ;
                        #endif
                        break;
                    case 2:
                    case 4:
                        wkBuffer = new byte[1];
                        wkBuffer[0] = COMSerialPort.ACK;
                        var res = ComPortInstance.PutBytes(null, wkBuffer);
                        if (res) result = 1;
                        #if DEBUG
                            result = 1;
                        #endif
                        break;
                    case 3:
                        //'データ受信
                        wkBuffer = new byte[100];
                        result = ComPortInstance.GetBytes(COMSerialPort.ETX, wkBuffer);
                        //'配列をワークに格納
                        #if DEBUG
                            result = 1;
                            debugText = "20000000500000000123000000500003";
                        #endif
                        break;
                    case 5:
                        //'データ受信
                        wkBuffer = new byte[1];
                        //ComPortInstance.Buffer = new byte[1];
                        result = ComPortInstance.GetBytes(COMSerialPort.EOT, wkBuffer);
                        #if DEBUG
                            result = 1;
                        #endif
                        break;
                    case 6:
                        sentByte = new byte[1];
                        sentByte[0] = COMSerialPort.ENQ;
                        if (ComPortInstance.PutBytes(null, sentByte)) result = 1;
                        #if DEBUG
                            result = 1;
                        #endif
                        break;
                    case 7:
                    case 9:
                        //'データ受信
                        wkBuffer = new byte[1];
                        result = ComPortInstance.GetBytes(COMSerialPort.ACK, wkBuffer);
                        ComPortInstance.Buffer[1] = wkBuffer[0];
                        #if DEBUG
                            result = COMSerialPort.ACK;
                        #endif
                        break;
                    case 8:
                        //'バイト配列の初期化
                        sentByte = new byte[6];
                        //'データ送信
                        sentByte[0] = COMSerialPort.STX;
                        wkBuffer = ASCIIEncoding.ASCII.GetBytes("9271");
                        for (int i = 0; i < 4; i++)
                        {
                            sentByte[i + 1] = wkBuffer[i];
                        }
                        sentByte[5] = COMSerialPort.ETX;
                        if (ComPortInstance.PutBytes(ASCIIEncoding.ASCII.GetString(sentByte), null)) result = 1;
                        #if DEBUG
                            result = 1;
                        #endif
                        break;
                    case 10:
                        //'バイト配列の初期化
                        wkBuffer = new byte[1];
                        wkBuffer[0] = COMSerialPort.EOT;
                        if (ComPortInstance.PutBytes(null, wkBuffer)) result = 1;
                        #if DEBUG
                            result = 1;
                        #endif
                        break;
                }
            }
            catch (Exception ex) 
            {
                ComPortInstance.Messages.Add(ex.Message);
                return -4;
            }
            return result;
        }
        public string SaveLog() 
        {
            var weight = 0d;
            //MeasurementLog
            //'１０ｇ単位の計量結果をｋｇに換算（五捨六入）
            //'ex)0557⇒5.57kg⇒小数点以下第一位を五捨六入して5kg
            if (!double.TryParse(ComPortInstance.WeightReadstring, out weight))
            {
                weight = 1;
            }
            else 
            {
                weight = Math.Round((weight / 1000) - 0.1, 0, MidpointRounding.AwayFromZero);
            }
            if (weight == 0) weight = 1;
            //'テキストファイルの保存
            //'コード 7bytes
            //'重量 4bytes
            //'日付 8bytes（YYYYMMDD形式）
            var dir = new DirectoryInfo(LogPassDirctory);
            if (!dir.Exists) dir.Create();
            if (string.IsNullOrEmpty(CodeInputText)) return "0";
            var file = new FileInfo(LogPassDirctory + @"\" + LogExecution);
            if (!file.Exists) return weight.ToString();
            //Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            //using (StreamWriter sw = File.AppendText(file.FullName))
            //{
            //    var b = Encoding.GetEncoding("shift_jis").GetBytes(string.Format("{0}{1}{2}\r\n", CodeInputText, weight.ToString("0000"), DateTime.Today.ToString("yyyyMMdd")));
            //    string text = Encoding.GetEncoding("shift_jis").GetString(b);
            //    sw.Write(text);
            //}
            ProcessStartInfo psi = new ProcessStartInfo("WScript.exe");
            psi.Arguments = string.Join(' ', new string[] { file.FullName, CodeInputText, weight.ToString() });
            psi.UserName = "takao";
            SecureString pass = new SecureString();
            foreach (char c in "takao") 
            {
                pass.AppendChar(c);
            }
            psi.Password = pass;
            Process vbs = Process.Start(psi);
            vbs.EnableRaisingEvents = false;
            vbs.ErrorDataReceived += Vbs_ErrorDataReceived;
            vbs.Exited += Vbs_Exited; ;
            return weight.ToString();
        }

        private void Vbs_Exited(object sender, EventArgs e)
        {
            AlertText += e.ToString();
        }

        private void Vbs_ErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            AlertText += e.Data;
        }

        public void RaisePropertyChanged([CallerMemberName]string propertyName = null)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
