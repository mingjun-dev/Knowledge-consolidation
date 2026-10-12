using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using DeviceMonitor.Models;
using DeviceMonitor.Services;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
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

        [ObservableProperty]
        private int count;

        [ObservableProperty]
        private int maxCount;


        private readonly IDeviceService _deviceService;

        private MockDeviceService mockDeviceService;

        CancellationTokenSource _cts;

        public MainViewModel(IDeviceService deviceService)
        {
            _deviceService = deviceService;
            _ = LoadDevicesAsync();
            mockDeviceService = new MockDeviceService();


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
            WeakReferenceMessenger.Default.Send(new Message.DeviceStatusMessage(SelectedDevice.Name, true, DateTime.Now, string.Empty));
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
            WeakReferenceMessenger.Default.Send(new Message.DeviceStatusMessage(SelectedDevice.Name, false, DateTime.Now, string.Empty));
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

        [RelayCommand]
        private async Task ReadAllAsync(CancellationToken ct)
        {
            _cts = new CancellationTokenSource();
            ct = _cts.Token;
            try
            {
                var devices = await mockDeviceService.GetDevicesAsync();
                MaxCount = devices.Count;
                Count = 0;
                var progress = new Progress<int>(value => Count = value);
                int completedCount = 0;
                var deviceName = devices.Select(async device =>
                {
                    var temperature = await mockDeviceService.ReadTemperatureAsync(device.Name, ct);
                    int done = Interlocked.Increment(ref completedCount);
                    ((IProgress<int>)progress).Report(done);
                    return temperature;
                });
                var results = await Task.WhenAll(deviceName);
            }
            catch (Exception ex)
            {
                Count = 0;
            }
        }
        [RelayCommand]
        private async Task CancelAsync()
        {
            _cts?.Cancel();
        }

        [RelayCommand]
        private void DemoWithoutLock()
        {

            var lists = new List<int>();
            var task = new Task[5];
            for (int i = 0; i < 5; i++)
            {
                task[i] = Task.Run(() =>
                {
                    for (int j = 0; j < 3000; j++)
                    {
                        try
                        {
                            lists.Add(j);
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(ex.Message);
                        }
                    }
                });
            }
            try
            {
                Task.WaitAll(task);
                MessageBox.Show($"List.Count = {lists.Count}，期望 5000");

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        private object _lock = new object();
        [RelayCommand]
        private void DemoWithLock()
        {
            var lists = new List<int>();
            var task = new Task[5];

            for (int i = 0; i < 5; i++)
            {
                task[i] = Task.Run(() =>
                {
                    for (int j = 0; j < 3000; j++)
                    {
                        lock (_lock)
                        {
                            lists.Add(j);
                        }
                    }
                });
            }
            Task.WaitAll(task);
            MessageBox.Show($"List.Count = {lists.Count}，期望 15000");
        }
        [RelayCommand]
        private void ConcurrentQueue()
        {
            var cq = new ConcurrentQueue<int>();
            var task = new Task[5];

            for (int i = 0; i < 5; i++)
            {
                task[i] = Task.Run(() =>
                {
                    for (int j = 0; j < 3000; j++)
                    {
                        cq.Enqueue(j);
                    }
                });
            }
            Task.WaitAll(task);
            MessageBox.Show($"List.Count = {cq.Count}，期望 15000");
        }

        [ObservableProperty]
        private string text = "默认值";

        [RelayCommand]
        private void Query()
        {
            Task.Run(() =>
            {
                Text = "后台修改";
            });
            
        }
    }
}
