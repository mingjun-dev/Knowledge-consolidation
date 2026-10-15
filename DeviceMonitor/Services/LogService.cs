using CommunityToolkit.Mvvm.Messaging;
using DeviceMonitor.Models;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeviceMonitor.Services
{
    public static class LogService
    {
        public static void Info(string message)
        {
            Log.Information(message);
            WeakReferenceMessenger.Default.Send(new Message.UiLogMessage(DateTime.Now, LogLevel.Info, message));
        }

        public static void Warn(string message)
        {
            Log.Warning(message);
            WeakReferenceMessenger.Default.Send(new Message.UiLogMessage(DateTime.Now, LogLevel.Warn, message));
        }

        public static void Error(string message, Exception? ex = null)
        {
            if (ex != null)
                Log.Error(ex, message);
            else
                Log.Error(message);

            WeakReferenceMessenger.Default.Send(new Message.UiLogMessage(DateTime.Now, LogLevel.Error, message));
        }

        public static void Debug(string message)
        {
            Log.Debug(message);
            WeakReferenceMessenger.Default.Send(new Message.UiLogMessage(DateTime.Now, LogLevel.Debug, message));
        }
    }
}
