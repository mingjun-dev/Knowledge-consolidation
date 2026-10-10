using DeviceMonitor.Services;
using DeviceMonitor.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using System;
using System.Configuration;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace DeviceMonitor
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        //全局唯一的日志框
        public static RichTextBox LogView = new RichTextBox()
        {
            IsReadOnly = true,
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
            Background = Brushes.Black,
            Foreground = Brushes.White,
            FontFamily = new FontFamily("Consolas")
        };

        public static IServiceProvider? ServiceProvider { get; private set; }

        private const string LogTemplate = "{Timestamp:yyyy-MM-dd HH:mm:ss fff} [{Level}] ({ThreadId}) {Message} {NewLine} {Excption}";

        private const string LogPath = "Logs\\log-.txt";
        protected override void OnStartup(StartupEventArgs e)
        {

            base.OnStartup(e);
            //Serilog全局配置
            Log.Logger = new LoggerConfiguration()
               .MinimumLevel.Debug()
                .Enrich.WithThreadId()
                .WriteTo.RichTextBox(LogView, outputTemplate: LogTemplate)
                .WriteTo.Console(outputTemplate: LogTemplate)
                .WriteTo.File(LogPath, rollingInterval: RollingInterval.Day, outputTemplate: LogTemplate, shared: true)
                .CreateLogger();

            var services = new ServiceCollection();

            services.AddSingleton<IDeviceService, MockDeviceService>();
            services.AddSingleton<MainViewModel>();
            services.AddSingleton<StatusBarViewModel>();
            services.AddSingleton<LogViewModel>();
            services.AddSingleton<MainWindow>();

            ServiceProvider = services.BuildServiceProvider();
            var mainWindow = ServiceProvider.GetRequiredService<MainWindow>();
            mainWindow.Show();
          
        }

        protected override void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);
            if (ServiceProvider is IDisposable disposable)
            {
                disposable.Dispose();
            }
        }
    }
    
}
