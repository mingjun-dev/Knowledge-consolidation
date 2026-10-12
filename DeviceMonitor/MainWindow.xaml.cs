using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DeviceMonitor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow(MainViewModel mainViewModel)
        {
            InitializeComponent();
            DataContext = mainViewModel;
        }
        private void Crash_Click(object sender, RoutedEventArgs e)
        {
            Task.Run(() =>
            {
                testText.Text = "我在后台线程改了你";
            });
        }
        private void Fix_Click(object sender, RoutedEventArgs e)
        {
            Task.Run(() =>
            {
                Thread.Sleep(500);

                Application.Current.Dispatcher.Invoke(() =>
                {
                    testText.Text = "Dispatcher 修复了 ✅";
                });
            });
        }
    }
}