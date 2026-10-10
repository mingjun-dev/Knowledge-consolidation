using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static DeviceMonitor.Models.Message;

namespace DeviceMonitor.ViewModels
{
    public class LogViewModel: ObservableObject
    {
        public ObservableCollection<string> LogItems { get; } = new();
        public LogViewModel()
        {
            LogItems.Add("系统启动完成");
            WeakReferenceMessenger.Default.Register<DeviceStatusMessage>(this, (_, msg) =>
            {
                string line = $"[{msg.Time:HH:mm:ss}] {msg.DeviceName} {(msg.IsConnected ? "连接成功" : "断开")}";
                LogItems.Add(line); 
            });
        }
    }
}
