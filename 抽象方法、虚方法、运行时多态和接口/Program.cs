using System.Data;

namespace 抽象方法_虚方法_运行时多态和接口
{
    public class Program
    {
        static void Main(string[] args)
        {
            List<DeviceBase> devices = new List<DeviceBase>();
            devices.Add(new PlcDevice { Name = "西门子" });
            devices.Add(new RobotDevice { Name = "汇川" });
            foreach (var item in devices)
            {
                item.Connect();
                item.Report();
                if (item is IConfigurable configurableDevice)
                {
                    configurableDevice.LoadConfig("config.xml");
                }
                else
                {
                    Console.WriteLine($"{item.Name}该设备不支持配置");
                }
            }



        }
    }
}
