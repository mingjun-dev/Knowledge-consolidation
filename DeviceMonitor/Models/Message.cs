using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace DeviceMonitor.Models
{
    public static class Message
    {

        public record DeviceStatusMessage(string DeviceName, bool IsConnected, DateTime Time, string Content);


        public record UiLogMessage(DateTime Time, LogLevel Level, string Text);
    }
    public enum LogLevel
    {
        Info,
        Warn,
        Error,
        Debug
    }

    public class LogEventArgs : EventArgs 
    {
        public DateTime Time { get;}
        public LogLevel Level { get; }
        public string Text { get; }

        public LogEventArgs(DateTime time, LogLevel level, string text)
        {
            Time = time;
            Level = level;
            Text = text;

        }
    }
}
