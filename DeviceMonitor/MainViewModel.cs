using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using DeviceMonitor.Models;
using DeviceMonitor.Services;
using Serilog;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Threading;
using static DeviceMonitor.Models.Message;

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

        [ObservableProperty]
        private double temperature;

        [ObservableProperty]
        private string statu;

        [ObservableProperty]
        private string status;

        [ObservableProperty]
        private DateTime? startTime;

        [ObservableProperty]
        private DateTime? endTime;

        private ObservableCollection<SensorRecord> QueryResult;

        [ObservableProperty]
        private long id;

        [ObservableProperty]
        private DateTime? recordTime;

        private readonly IDeviceService _deviceService;

        private MockDeviceService mockDeviceService;

        private CancellationTokenSource _cts;

        private ModbusService modbusService;


        public MainViewModel(IDeviceService deviceService)
        {
            _deviceService = deviceService;
            _ = LoadDevicesAsync();
            mockDeviceService = new MockDeviceService();
            QueryResult = new ObservableCollection<SensorRecord>();


            //_timer = new DispatcherTimer();
            //_timer.Interval = TimeSpan.FromSeconds(3); //1秒执行一次
            //_timer.Tick += Timer_Tick;
            //_timer.Start(); //启动
        }

        [RelayCommand]
        private async Task HistoryQueryAsync()
        {
            var list =  await QueryDbAsync(StartTime ?? DateTime.Now, EndTime ?? DateTime.Now);
            QueryResult.Clear();
            foreach (var record in list) 
            {
                QueryResult.Add(record);
            }
        }

        [RelayCommand]
        private async Task StartDbTest()
        {
            Stopwatch sw = Stopwatch.StartNew();
            Random rand = new Random();
            for (int i = 0; i < 1000; i++)
            {
                var record = new SensorRecord
                {
                    Temperature = rand.Next(20, 50),
                    RecordTime = DateTime.Now
                };
                await SqliteDbService.InsertAsync(Temperature, RecordTime ?? DateTime.Now);
                await Task.Delay(1000);
            }
           
            sw.Stop();
            Log.Information($"写入1000条完成，总耗时：{sw.ElapsedMilliseconds} ms");
        }
        private async Task<List<SensorRecord>> QueryDbAsync(DateTime start, DateTime end)
        {
            var list = await SqliteDbService.QuerytAsync(start, end);
            return list;
        }
        //private void Timer_Tick(object? sender, EventArgs e)
        //{
        //    StatusText = "共用户端设备：" + Devices.Count + "台";
        //}

        [RelayCommand(CanExecute = nameof(CanConnect))]
        private async Task ConnectAsync()
        {

            //if (SelectedDevice is null)
            //{
            //    StatusText = "请先选择一台设备";
            //    return;
            //}
            try
            {
                SelectedDevice.IsConnected = true;
                IsConnected = true;
                WeakReferenceMessenger.Default.Send(new Message.DeviceStatusMessage(SelectedDevice.Name, true, DateTime.Now, ""));
                CollectionViewSource.GetDefaultView(Devices).Refresh();
                try
                {


                    modbusService = new ModbusService();
                    await modbusService.ConnectAsync("127.0.0.1", 502, 3000);
                    Statu = "已连接";
                    _cts = new CancellationTokenSource();
                    var ct = _cts.Token;
                    _ = Task.Run(() => PollLoopAsync(ct));
                }
                catch (Exception)
                {

                    throw;
                }
            }
            catch (Exception ex)
            {
                WeakReferenceMessenger.Default.Send(new Message.DeviceStatusMessage("", false, DateTime.Now, $"{ex.Message}"));
            }

        }

        [RelayCommand]
        private async Task DisconnectAsync()
        {
            //if (SelectedDevice is null)
            //{
            //    StatusText = "请先选择一台设备";
            //    return;
            //}
            _cts.Cancel();
            SelectedDevice.IsConnected = false;
            IsConnected = false;
            WeakReferenceMessenger.Default.Send(new Message.DeviceStatusMessage(SelectedDevice.Name, false, DateTime.Now, ""));
            CollectionViewSource.GetDefaultView(Devices).Refresh();

            try
            {
                modbusService?.Disconnect();
                modbusService = null;
            }
            catch (Exception)
            {

            }


        }
        private async Task PollLoopAsync(CancellationToken ct)
        {
            while (!ct.IsCancellationRequested)
            {
                try
                {
                    Temperature = await modbusService.ReadTemperatureAsync(1, 0);
                    Status = "读取成功";
                    try
                    {
                        await SqliteDbService.InsertAsync(Temperature, DateTime.Now);
                    }
                    catch (Exception)
                    {

                    }
                }
                catch (Exception)
                {
                    Status = "读取失败";
                    WeakReferenceMessenger.Default.Send(new Message.DeviceStatusMessage("", false, DateTime.Now, "温度读取失败"));
                }
                try
                {
                    await Task.Delay(200, ct);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
            }
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
        private async Task ReadAllAsync()
        {
            _cts?.Cancel();
            _cts?.Dispose();
            _cts = new CancellationTokenSource();
            var ct = _cts.Token;
            try
            {
                var devices = await mockDeviceService.GetDevicesAsync();
                MaxCount = devices.Count;
                Count = 0;
                var progress = new Progress<int>(value => Count = value);
                int completedCount = 0;
                var deviceName = devices.Select(async device =>
                {
                    ct.ThrowIfCancellationRequested();
                    var temperature = await mockDeviceService.ReadTemperatureAsync(device.Name, ct);
                    int done = Interlocked.Increment(ref completedCount);
                    ((IProgress<int>)progress).Report(done);
                    return temperature;
                });
                var results = await Task.WhenAll(deviceName);
            }
            catch (OperationCanceledException o)
            {
                Count = 0;
                WeakReferenceMessenger.Default.Send(new DeviceStatusMessage("", false, DateTime.Now, $"读取温度失败:原因{o.Message}"));
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
