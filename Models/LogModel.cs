using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IngicateWpf
{
    public class LogModel
    {
        public DateTime EventOccours { get; set; }
        public string EventSource { get; set; }
        public string EventMessage { get; set; }
        public ErrorModel Error { get; set; }
        public string Memo { get; set; }
    }
    public class ErrorModel
    {
        public DateTime ErrorOccours { get; set; }
        public string ErrorSource { get; set; }
        public string ErrorMessage { get; set; }
    }
    public enum LogType
    {
        Default = 0,
        PassCheck = 1,
        measurement = 2
    }
    public class WeightMeasurementLog : LogModel
    {
        double _weight { get; set; }
        public double Weight 
        {
            get 
            {
                return _weight;
            }
            set 
            {
                _weight = value;
            }
        }
        public string WeightString{ get; set; }
        public string Code
        {
            get
            {
                return EventSource;
            }
            set
            {
                EventSource = value;
            }
        }
        public DateTime Time
        {
            get
            {
                return EventOccours;
            }
            set
            {
                EventOccours = value;
            }
        }
    }
    public enum ComCommunicateResultType 
    {
        None,
        TimeOut,
        Cnacel,
        Success,
        Exception
    }
}
