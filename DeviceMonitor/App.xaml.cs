using DeviceMonitor.Models;
using DeviceMonitor.Services;
using DeviceMonitor.ViewModels;
using Microsoft.Extensions.Configuration;
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

        //string  connectionString = "Data Source=DeviceMonitor.db;";
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

           
           

            //① 先构建出 IConfiguration 对象
            var configBuilder = new ConfigurationBuilder()
                .SetBasePath(System.IO.Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
                 IConfiguration config = configBuilder.Build();

            var services = new ServiceCollection();
            services.AddSingleton<IDeviceService, MockDeviceService>();
            services.AddSingleton<StatusBarViewModel>();
            services.AddSingleton<LogViewModel>();
            services.AddSingleton<MainWindow>();
            //② 把原生IConfiguration注册进DI容器
            services.AddSingleton(config);
            //③ 注册IConfigService，ConfigService依赖IConfiguration
            services.AddSingleton<IConfigService, ConfigService>();
            services.AddSingleton<MainViewModel>();

            ServiceProvider = services.BuildServiceProvider();

            var configService = ServiceProvider.GetRequiredService<IConfigService>();

            var dbOption = configService.GetDbConfig();
            string connectionString = $"Data Source={dbOption.DbFilePath}";
            SqliteDbService.Initialize(connectionString);

            
            var mainWindow = ServiceProvider.GetRequiredService<MainWindow>();
            mainWindow.Show();
           
        }

        protected override void OnExit(ExitEventArgs e)
        {
            Log.CloseAndFlush();
            base.OnExit(e);
            if (ServiceProvider is IDisposable disposable)
            {
                disposable.Dispose();
            }
        }
    }
    
}
