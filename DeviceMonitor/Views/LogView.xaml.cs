using CommunityToolkit.Mvvm.Messaging;
using DeviceMonitor.Models;
using DeviceMonitor.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DeviceMonitor.Views
{
    /// <summary>
    /// LogView.xaml 的交互逻辑
    /// </summary>
    public partial class LogView : UserControl
    {
        public LogView()
        {
            InitializeComponent();
            if (DesignerProperties.GetIsInDesignMode(this))
                return;
            DataContext = App.ServiceProvider.GetRequiredService<ViewModels.LogViewModel>();
            Loaded += (s, e) =>
            {
                //①接收普通日志消息：来自LogService发送的UiLogMessage
                WeakReferenceMessenger.Default.Register<Message.UiLogMessage>(this, (_, msg) =>
                {
                    Dispatcher.Invoke(() =>
                    {
                        if (DataContext is LogViewModel vm)
                        {
                            string line = $"[{msg.Time:HH:mm:ss}] [{msg.Level}] {msg.Text}";
                            vm.AddLog(line);
                            LogScroll.ScrollToBottom();
                        }
                    });
                });

                //②接收设备状态消息（连接断开事件，额外写入日志）
                WeakReferenceMessenger.Default.Register<Message.DeviceStatusMessage>(this, (_, msg) =>
                {
                    Dispatcher.Invoke(() =>
                    {
                        if (DataContext is LogViewModel vm)
                        {
                            string stateText = msg.IsConnected ? "连接成功" : "已断开";
                            string line = $"[{msg.Time:HH:mm:ss}] [Info] {msg.DeviceName} {stateText}，{msg.Content}";
                            vm.AddLog(line);
                            LogScroll.ScrollToBottom();
                        }
                    });
                });
            };

            Unloaded += (s, e) =>
            {
                //控件销毁，注销订阅，防止内存泄漏、重复收到多条消息
                WeakReferenceMessenger.Default.UnregisterAll(this);
            };
        }
    }
}
