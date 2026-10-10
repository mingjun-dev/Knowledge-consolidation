using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using DeviceMonitor.Models;
using DeviceMonitor.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Threading;

namespace DeviceMonitor
{
    public partial class MainViewModel : ObservableObject
    {
        //DispatcherTimer _timer;
        public ObservableCollection<Device> Devices { get; } = new ObservableCollection<Device>();

        [ObservableProperty]
        private Device selectedDevice;

        [ObservableProperty]
        private bool isConnected;

        private readonly IDeviceService _deviceService;

        public MainViewModel(IDeviceService deviceService)
        {
            _deviceService = deviceService;
            _ = LoadDevicesAsync();
            //_timer = new DispatcherTimer();
            //_timer.Interval = TimeSpan.FromSeconds(3); //1秒执行一次
            //_timer.Tick += Timer_Tick;
            //_timer.Start(); //启动
        }

        //private void Timer_Tick(object? sender, EventArgs e)
        //{
        //    StatusText = "共用户端设备：" + Devices.Count + "台";
        //}

        [RelayCommand(CanExecute = nameof(CanConnect))]
        private void Connect()
        {
            //if (SelectedDevice is null)
            //{
            //    StatusText = "请先选择一台设备";
            //    return;
            //}
            SelectedDevice.IsConnected = true;
            IsConnected = true;
            WeakReferenceMessenger.Default.Send(new Message.DeviceStatusMessage(SelectedDevice.Name, true, DateTime.Now));
            CollectionViewSource.GetDefaultView(Devices).Refresh();
        }

        [RelayCommand]
        private void Disconnect()
        {
            //if (SelectedDevice is null)
            //{
            //    StatusText = "请先选择一台设备";
            //    return;
            //}
            SelectedDevice.IsConnected = false;
            IsConnected = false;
            WeakReferenceMessenger.Default.Send(new Message.DeviceStatusMessage(SelectedDevice.Name, false, DateTime.Now));
            CollectionViewSource.GetDefaultView(Devices).Refresh();
        }


        partial void OnSelectedDeviceChanged(Device value)
        {
            //StatusText = $"当前选择设备：{value.Name}，IP地址：{value.IpAddress}";
            //_timer.Stop();
            ConnectCommand.NotifyCanExecuteChanged();
            DisconnectCommand.NotifyCanExecuteChanged();
        }

        private bool CanConnect()
        {
            if (SelectedDevice is null) return false;
            return true;
        }

        private async Task LoadDevicesAsync() 
        {
            var list = await _deviceService.GetDevicesAsync();
            foreach (var device in list)
            {
                Devices.Add(device);
            }
        }
    }
}
