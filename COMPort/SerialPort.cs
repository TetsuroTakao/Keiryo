﻿using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Ports;
using System.Text;
using System.Threading;
using System.Linq;

namespace IngicateWpf.COMPort
{
    public class CommException : ApplicationException
    {
        public CommException(string Reason)
        {
            this.Source = Reason;
        }
    }
    public struct COMMTIMEOUTS
    {
        public Int32 ReadIntervalTimeout { get; set; }
        public Int32 ReadTotalTimeoutMultiplier { get; set; }
        public Int32 ReadTotalTimeoutConstant { get; set; }
        public Int32 WriteTotalTimeoutMultiplier { get; set; }
        public Int32 WriteTotalTimeoutConstant { get; set; }
    }
    public class COMSerialPort
    {
        //'シリアルポート通信 制御用コード
        public const int ENQ = 0x5;//'待ち受け宣言
        public const int ACK= 0x6;//'肯定信号
        public const int NAK = 0x15;//     '？？？
        public const int STX= 0x2;//'テキスト送信開始宣言
        public const int ETX= 0x3;//'テキスト送信終了宣言
        public const int EOT= 0x4;//'通信終宣言
        public List<string> Messages { get; set; }
        public ComCommunicateResultType Result { get; set; }
        SerialPort serialPort { get; set; }
        public bool ReadComplete { get; set; }
        public string ComPortName { get; set; }
        public string WeightReadstring { get; set; }
        public byte[] WeightReadBuffer { get; set; }
        public byte[] Buffer { get; set; }
        public COMSerialPort(string port) 
        {
            Messages = new List<string>();
            Buffer = new byte[100];
            ComPortName = port;
        }
        public void Open() 
        {
            try
            {
                Messages.Add("Accessing the COM1 serial port");
                if (serialPort != null)
                {
                    if (!serialPort.IsOpen)
                    {
                        serialPort = new SerialPort(ComPortName, 2400, Parity.Even, 7, StopBits.Two);
                        serialPort.DataReceived += SerialPort_DataReceived;
                        serialPort.ReadTimeout = 50;
                        serialPort.WriteTimeout = 50;
                        serialPort.ErrorReceived += SerialPort_ErrorReceived;
                    }
                }
                else
                {
                    serialPort = new SerialPort(ComPortName, 2400, Parity.Even, 7, StopBits.Two);
                    serialPort.Open();
                    serialPort.DataReceived += SerialPort_DataReceived;
                    serialPort.ReadTimeout = 50;
                    serialPort.WriteTimeout = 50;
                    serialPort.ErrorReceived += SerialPort_ErrorReceived;
                }
            }
            catch (UnauthorizedAccessException _unauthorizedAccessException)
            {
                Messages.Add(_unauthorizedAccessException.Message);
            }
            catch (ArgumentOutOfRangeException _argumentOutOfRangeException)
            {
                Messages.Add(_argumentOutOfRangeException.Message);
            }
            catch (ArgumentException _argumentException)
            {
                Messages.Add(_argumentException.Message);
            }
            catch (IOException _iOException)
            {
                Messages.Add(_iOException.Message);
            }
            catch (InvalidOperationException _invalidOperationException)
            {
                Messages.Add(_invalidOperationException.Message);
            }
            catch (Exception ex)
            {
                Messages.Add(ex.Message);
            }
            finally 
            {
                //serialPort.Close();
            }
        }
        private void SerialPort_ErrorReceived(object sender, SerialErrorReceivedEventArgs e)
        {
            Messages.Add(e.EventType.ToString());
        }
        private void SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            Messages.Add(e.EventType.ToString());
        }
        public void Close() 
        {
//#if !DEBUG
            try 
            {
                if (serialPort.IsOpen)
                {
                    serialPort.Close();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
//#endif
        }
        public int GetBytes(int check,byte[] buffer) 
        {
            var result = 0;
            result = -1;
            ReadComplete = false;
            try 
            {
                if (serialPort == null) Open();
                if (!serialPort.IsOpen) serialPort.Open();
                do
                {
                    result = (serialPort.BytesToRead > 0) ? serialPort.Read(buffer, 0, buffer.Length) : 0;
                    if (Result == ComCommunicateResultType.Cnacel)
                    {
                        result = -2;
                        ReadComplete = true;
                        break;
                    }
                    if (check == ETX) 
                    {
                        if (buffer[0] != STX)
                        {
                            do
                            {
                                result = (serialPort.BytesToRead > 0) ? serialPort.Read(buffer, 0, buffer.Length) : 0;
                            } while (buffer[0] != STX);
                        }
                        else 
                        {
                            var r = buffer.Where(b => b != 0);
                            if (r.Count() > 0)
                            {
                                WeightReadstring = ASCIIEncoding.ASCII.GetString(r.ToArray());
                                if (WeightReadstring.Length > 20)
                                {
                                    WeightReadstring = WeightReadstring.Substring(14, 6);
                                    ReadComplete = true;
                                    break;
                                }
                                else
                                {
                                    ReadComplete = false;
                                }
                            }
                        }
                    }
                    else
                    {
                        if (buffer[0] == check)
                        {
                            result = check;
                            ReadComplete = true;
                            break;
                        }
                    }
                } while (!ReadComplete);
            }
            catch (Exception ex) 
            {
                Messages.Add(ex.Message);
            }
            return result;
        }
        public bool PutBytes(string writeData,byte[] byteArg) 
        {
            var result = true;
//#if !(DEBUG)
            if (serialPort == null) Open();
            //if (!serialPort.IsOpen) Open();
            result = false;
            try 
            {
                if (string.IsNullOrEmpty(writeData))
                {
                    //serialPort.Write(byteArg,0, byteArg.Length);
                    serialPort.Write(byteArg, 0, byteArg.Length);
                }
                else 
                {
                    serialPort.Write(writeData);
                }
                result = true;
            }
            catch (Exception ex) 
            {
                Messages.Add(ex.Message);
            }
//#endif
            return result;
        }
    }
}
