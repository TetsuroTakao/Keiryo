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
        public string LogSaveDirctory { get; set; }
        public string LogSaveFileName { get; set; }
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
                if (result >= 0)
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
//#if DEBUG
//                    ComPortInstance.Messages.Add(strText);
//#endif
                    if (result == -3) break;
                    if (j > 10) break;
                }
                else 
                {
                    if (j > 4) break;
                }
            } while (result < 0);
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
                //ComPortInstance.Buffer = new byte[100];
                if (step == 1 || step == 3 || step == 5 || step == 7 || step == 9) 
                {
                    switch (step)
                    {
                        case 1:
                            //'データ受信
                            wkBuffer = new byte[1];
                            //ComPortInstance.Buffer = new byte[1];
                            result = ComPortInstance.GetBytes(COMSerialPort.ENQ, wkBuffer);

                            ComPortInstance.Buffer[1] = wkBuffer[0];
//#if DEBUG
//                            result = COMSerialPort.ENQ;
//#endif
                            break;
                        case 3:
                            //'データ受信
                            //ComPortInstance.Buffer = new byte[100];
                            wkBuffer = new byte[100];
                            do
                            {
                                result = ComPortInstance.GetBytes(COMSerialPort.STX, wkBuffer);
                            } while (result != 0);
                            //'配列をワークに格納
                            debugText = string.Empty;
                            for (int j = 0; j < wkBuffer.Length; j++)
                            {
                                debugText += wkBuffer[j].ToString() + ",";
                                //'ETX検出したら終了
                                //if (ComPortInstance.Buffer[j] == COMSerialPort.ETX)
                                if (wkBuffer[j] == COMSerialPort.ETX)
                                {
                                    result = -3;
                                    //'がばっとコピーし、objComPortに受信テキストだけ入った状態に
                                    //ComPortInstance.Buffer = wkBuffer;
                                    ComPortInstance.WeightReadBuffer = wkBuffer;
                                }
                                //else
                                //{
                                //    //'データを1バイトずつコピー
                                //    wkBuffer[j - 1] = ComPortInstance.Buffer[j];
                                //}
                            }
                            var wkBuffer2 = new byte[wkBuffer.Length];
                            var m = 0;
                            strText = string.Empty;
                            foreach (var b in wkBuffer) 
                            {
                                strText += b.ToString() + ",";
                                wkBuffer2[m] = b;
                                m++;
                            }
                            strText = ASCIIEncoding.ASCII.GetString(wkBuffer2);
                            //strText = UnicodeEncoding.ASCII.GetString(wkBuffer);
//#if DEBUG
//                            //Thread.Sleep(10000);
//                            result = -3;
//                            strText = "01580" + "kkkkkk";
//                            //24 81 08 27 24 87 72 48 28
//                            strText = "01060" + "kkkkkk";
//                            //481082724877248280
//                            strText = "00420" + "kkkkkk";
//                            wkBuffer = UnicodeEncoding.ASCII.GetBytes(strText);
//                            strText = UnicodeEncoding.ASCII.GetString(wkBuffer);
//                            ComPortInstance.WeightReadBuffer = wkBuffer;
//#endif
                            ComPortInstance.Messages.Add(strText);
                            //ComPortInstance.WeightReadBuffer = wkBuffer;
                            break;
                        case 5:
                            //'データ受信
                            wkBuffer = new byte[1];
                            //ComPortInstance.Buffer = new byte[1];
                            result = ComPortInstance.GetBytes(COMSerialPort.EOT, wkBuffer);
//#if DEBUG
//                            result = COMSerialPort.EOT;
//#endif
                            break;
                        case 7:
                        case 9:
                            //'データ受信
                            wkBuffer = new byte[1];
                            //ComPortInstance.Buffer = new byte[1];
                            result = ComPortInstance.GetBytes(COMSerialPort.ACK, wkBuffer);
//#if DEBUG
//                            result = COMSerialPort.ACK;
//#endif
                            break;
                    }
                }
                if (step == 2 || step == 4 || step == 6 || step == 8 || step == 10)
                {
                    //'バイト配列の初期化
                    ComPortInstance.Buffer = new byte[0];
                    switch (step)
                    {
                        case 2:
                        case 4:
                            sentByte = new byte[1];
                            sentByte[0] = COMSerialPort.ACK;
                            if (ComPortInstance.PutBytes(null, sentByte)) result = 0;
//#if DEBUG
//                            result = 0;
//#endif
                            break;
                        case 6:
                            sentByte = new byte[1];
                            sentByte[0] = COMSerialPort.ENQ;
                            if (ComPortInstance.PutBytes(null, sentByte)) result = 0;
//#if DEBUG
//                            result = 0;
//#endif
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
                            if (ComPortInstance.PutBytes(ASCIIEncoding.ASCII.GetString(sentByte),null)) result = 0;
//#if DEBUG
//                            result = 0;
//#endif
                            break;
                        case 10:
                            //'バイト配列の初期化
                            sentByte = new byte[1];
                            sentByte[0] = COMSerialPort.EOT;
                            if (ComPortInstance.PutBytes(null, sentByte)) result = 0;
                            //'テキストから重量部分を切り出し
                            if (strText.Length > 5)
                            {
                                strText = strText.Substring(0, 5);
                            }
                            break;
                    }
                }
            }
            catch (Exception ex) 
            {
                ComPortInstance.Messages.Add(ex.Message);
                return -4;
            }
            return result;
        }
        public int SaveLog(string strCode, string strKeiryoKekka) 
        {
            var weight = 0d;
            //MeasurementLog
            //'１０ｇ単位の計量結果をｋｇに換算（五捨六入）
            //'ex)0557⇒5.57kg⇒小数点以下第一位を五捨六入して5kg
            if (!double.TryParse(strKeiryoKekka, out weight))
            {
                weight = 1;
            }
            else 
            {
                weight = Math.Round((weight / 1000) - 0.01, 0, MidpointRounding.AwayFromZero);
            }
            if (weight == 0) weight = 1;
            //'テキストファイルの保存
            //'コード 7bytes
            //'重量 4bytes
            //'日付 8bytes（YYYYMMDD形式）
            var dir = new DirectoryInfo(LogSaveDirctory);
            if (!dir.Exists) dir.Create();
            if (string.IsNullOrEmpty(strCode)) return 0;
            var file = new FileInfo(LogSaveDirctory + @"\" + LogSaveFileName);
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            using (StreamWriter sw = File.AppendText(file.FullName))
            {
                var b = Encoding.GetEncoding("shift_jis").GetBytes(string.Format("{0}{1}{2}\r\n", strCode, weight.ToString("0000"), DateTime.Today.ToString("yyyyMMdd")));
                string text = Encoding.GetEncoding("shift_jis").GetString(b);
                sw.Write(text);
            }
            return (int)weight;
        }
        public void RaisePropertyChanged([CallerMemberName]string propertyName = null)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
