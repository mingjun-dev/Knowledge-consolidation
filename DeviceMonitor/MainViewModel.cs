using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace DeviceMonitor
{
    public partial class MainViewModel : ObservableObject
    {
        DispatcherTimer _timer;
       public ObservableCollection<Device> Devices { get; set; }

        [ObservableProperty] 
        private Device selectedDevice;
       

        [ObservableProperty]
        private string statusText;


        public MainViewModel()
        {
            Devices = new ObservableCollection<Device>();
            Devices.Add(new Device
            {
                Name = "PLC‑001",
                IpAddress = "192.168.1.101",
                IsConnected = false
            });

            Devices.Add(new Device
            {
                Name = "PLC‑002",
                IpAddress = "192.168.1.102",
                IsConnected = false
            });

            Devices.Add(new Device
            {
                Name = "Robot‑001",
                IpAddress = "192.168.1.201",
                IsConnected = false
            });

            Devices.Add(new Device
            {
                Name = "Robot‑002",
                IpAddress = "192.168.1.202",
                IsConnected = false
            });

            Devices.Add(new Device
            {
                Name = "Camera‑001",
                IpAddress = "192.168.1.301",
                IsConnected = false
            });


            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromSeconds(3); //1秒执行一次
            _timer.Tick += Timer_Tick;
            _timer.Start(); //启动
        }

        private void Timer_Tick(object? sender, EventArgs e)
        {
            StatusText = "共用户端设备：" + Devices.Count + "台";
        }

        [RelayCommand(CanExecute =nameof(CanConnect))]
        private void Connect()
        {
            if (SelectedDevice is null) 
            {
                StatusText = "请先选择一台设备";
                return;
            }
            SelectedDevice.IsConnected = true;
            StatusText = "设备 " + SelectedDevice.Name + " 已连接，IP地址：" + SelectedDevice.IpAddress;
        }

        [RelayCommand]
        private void Disconnect()
        {
            if (SelectedDevice is null)
            {
                StatusText = "请先选择一台设备";
                return;
            }
            SelectedDevice.IsConnected = false;
            StatusText = "设备 " + SelectedDevice.Name + " 已断开连接，IP地址：" + SelectedDevice.IpAddress;
        }


        partial void OnSelectedDeviceChanged(Device value)
        {
            StatusText = "$当前选择设备：" + value.Name + "，IP地址：" + value.IpAddress;
            _timer.Stop();
            ConnectCommand.NotifyCanExecuteChanged();
            DisconnectCommand.NotifyCanExecuteChanged();
        }

        private bool CanConnect() 
        {
            if(SelectedDevice is null) return false;
            return true;
        }
    }
}
