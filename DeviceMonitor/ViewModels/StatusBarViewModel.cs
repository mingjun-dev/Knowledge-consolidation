using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using DeviceMonitor.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeviceMonitor.ViewModels
{
    public partial class StatusBarViewModel: ObservableObject
    {
        [ObservableProperty]
        private string statusText;

        
        public StatusBarViewModel()
        {
            WeakReferenceMessenger.Default.Register<Message.DeviceStatusMessage>(this, (r, m) =>
            {
                StatusText = $"{m.DeviceName} {(m.IsConnected ? "已连接" : "已断开")} {m.Time.ToString("HH:mm:ss")}";
            });
        }

        [RelayCommand]
        private void Query()
        {
            Task.Run(() =>
            {
                for (int i = 0; i < 1000; i++)
                {
                    StatusText = $"我在后台线程改了你 {i}";
                }
            });
        }
    }
}
