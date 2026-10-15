using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using DeviceMonitor.Models;
using DeviceMonitor.Services;
using LiveCharts;
using LiveCharts.Defaults;
using LiveCharts.Wpf;
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
using System.Windows.Media;
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
        private string pauseOrContinue = "暂停图表";

        private bool _isChartPause;

        [ObservableProperty]
        private string statu;

        [ObservableProperty]
        private string status;

        [ObservableProperty]
        private DateTime? startTime;

        [ObservableProperty]
        private DateTime? endTime;

        public ObservableCollection<SensorRecord> QueryResult { get; set; }

        [ObservableProperty]
        private long id;

        [ObservableProperty]
        private DateTime? recordTime;

        [ObservableProperty]
        private SeriesCollection tempLiveCharts;

        private readonly IDeviceService _deviceService;

        private MockDeviceService mockDeviceService;

        private CancellationTokenSource _cts;

        private ModbusService modbusService;

        private readonly IConfigService _configService;

        public MainViewModel(IDeviceService deviceService, IConfigService configService)
        {
            _deviceService = deviceService;
            _ = LoadDevicesAsync();
            mockDeviceService = new MockDeviceService();
            QueryResult = new ObservableCollection<SensorRecord>();
            TempLiveCharts = new SeriesCollection()
            {
                new LineSeries
                {
                    Title="温度趋势",
                    Values = new ChartValues<ObservablePoint>(),
                    Fill = Brushes.Gray,
                    Stroke = Brushes.Blue,
                    StrokeThickness = 1,
                    PointGeometry = null,
                    LineSmoothness = 0
                }
            };
            _configService = configService;


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
            var record = new SensorRecord
            {
                Temperature = rand.Next(20, 50),
                RecordTime = DateTime.Now
            };
            await SqliteDbService.InsertAsync(record.Temperature, record.RecordTime);

            sw.Stop();
            
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
                if (SelectedDevice is null)
                {
                    Statu = "请先选择设备";
                    LogService.Warn("未选择设备，拒绝连接");
                    return;
                }

                CollectionViewSource.GetDefaultView(Devices).Refresh();

                PlcOption plcOption = _configService.GetPlcConfig();
                modbusService = new ModbusService();

                //真正网络连接
                await modbusService.ConnectAsync(plcOption.IpAddress, plcOption.Port, 3000);

                //✅连接成功之后，再修改状态！
                SelectedDevice.IsConnected = true;
                IsConnected = true;
                WeakReferenceMessenger.Default.Send( new Message.DeviceStatusMessage("温度PLC", true, DateTime.Now, "Modbus握手完成"));

                Statu = "已连接";
                LogService.Info($"PLC连接成功 Ip={plcOption.IpAddress}");

                _cts = new CancellationTokenSource();
                var ct = _cts.Token;
                _ = Task.Run(() => PollLoopAsync(ct));
            }
            catch (Exception ex)
            {
                Statu = "连接失败";
                LogService.Error("PLC连接失败", ex);
                WeakReferenceMessenger.Default.Send(new Message.DeviceStatusMessage("", false, DateTime.Now, $"{ex.Message}"));

                //连接失败，清理资源
                modbusService?.Disconnect();
                modbusService = null;
                _cts?.Cancel();
                _cts?.Dispose();
                _cts = null;
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
            try
            {
                if (SelectedDevice != null)
                {
                    SelectedDevice.IsConnected = false;
                    WeakReferenceMessenger.Default.Send(new Message.DeviceStatusMessage(SelectedDevice.Name, false, DateTime.Now, ""));
                }

                IsConnected = false;
                CollectionViewSource.GetDefaultView(Devices).Refresh();

                modbusService?.Disconnect();
                modbusService = null;

                Statu = "已断开";
                LogService.Info("PLC断开成功");
            }
            catch (Exception ex)
            {
                LogService.Error("PLC断开发生异常", ex);
                Statu = "断开异常";
            }
            finally
            {
                if (_cts != null)
                {
                    _cts.Cancel();
                    _cts.Dispose();
                    _cts = null; //必须置空
                }
            }
        }
        private async Task PollLoopAsync(CancellationToken ct)
        {
            LogService.Info("轮询读取温度任务已启动");
            while (!ct.IsCancellationRequested)
            {
                try
                {
                    Temperature = await modbusService.ReadTemperatureAsync(1, 0);
                    Status = "读取成功";
                    LogService.Info($"温度读取成功，当前温度:{Temperature}");

                    try
                    {
                        await SqliteDbService.InsertAsync(Temperature, DateTime.Now);
                        await Task.Delay(500, ct);
                        AddChartPoint(Temperature, DateTime.Now);
                    }
                    catch (Exception ex)
                    {
                        LogService.Warn("Sqlite数据库插入温度记录失败");
                    }

                    //✅ 把Delay移到外层try内部，不再单独套try‑catch
                    await Task.Delay(200, ct);
                }
                catch (OperationCanceledException)
                {
                    //手动断开触发的正常取消，走到这里
                    LogService.Info("轮询任务收到取消信号，正常退出循环");
                    break;
                }
                catch (Exception ex)
                {
                    //真正业务异常：读modbus、数据库以外的报错
                    Status = "读取失败";
                    LogService.Error("Modbus读取温度发生异常", ex);
                    WeakReferenceMessenger.Default.Send(new Message.DeviceStatusMessage("", false, DateTime.Now, "温度读取失败"));
                }
            }
            LogService.Info("温度轮询循环已经完全退出");
        }
        [RelayCommand]
        private void ClearChart() 
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                _isChartPause = false;
                PauseOrContinue = "暂停图表";
               
                var lineSeries = TempLiveCharts[0] as LineSeries;
                if (lineSeries != null)
                {
                    lineSeries.Values.Clear();
                   
                }
            });
        }
        [RelayCommand]
        private void StartOrStop() 
        {
            _isChartPause = !_isChartPause;
            PauseOrContinue = _isChartPause ? "继续图表" : "暂停图表";
        }
        private void AddChartPoint(double temp, DateTime time, int maxPoints = 100)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                if (_isChartPause) return;
                if (TempLiveCharts.Count == 0) return;

                var lineSeries = TempLiveCharts[0] as LineSeries;
                if (lineSeries == null) return;

               
                var point = new ObservablePoint((double)time.Ticks, temp);
                lineSeries.Values.Add(point);

                while (lineSeries.Values.Count > maxPoints)
                {
                    lineSeries.Values.RemoveAt(0);
                }
            });
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
